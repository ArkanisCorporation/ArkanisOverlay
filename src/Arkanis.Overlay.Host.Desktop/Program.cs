namespace Arkanis.Overlay.Host.Desktop;

using System.Globalization;
using Windows.Win32;
using Common.Abstractions;
using Common.Enums;
using Common.Extensions;
using Components.Helpers;
using Components.Services;
using Dapplo.Microsoft.Extensions.Hosting.AppServices;
using Dapplo.Microsoft.Extensions.Hosting.Wpf;
using Domain.Abstractions.Services;
using Helpers;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.Extensions;
using Infrastructure.Services;
using Infrastructure.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Quartz;
using Serilog;
using Services;
using Services.Factories;
using UI;
using UI.Windows;
using Velopack;
using Velopack.Sources;
using Workers;

// based on:
// https://github.com/dapplo/Dapplo.Microsoft.Extensions.Hosting/blob/master/samples/Dapplo.Hosting.Sample.WpfDemo/Program.cs#L48
public static class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        // WPF Applications do not allocate a console by default, nor do they attach to the parent console if one exists.
        // To get proper console output, we need to attach to the parent console.
        // This is safe to use even if there is no parent console or process - it just won't have any effect.
        PInvoke.AttachConsole(PInvoke.ATTACH_PARENT_PROCESS);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        await HandleInstallationBehaviourAsync(args);

        var hostBuilder = Host.CreateDefaultBuilder(args)
            .UseCommonServices((context, options) =>
                {
                    options.UseFileLogging = true;
                    options.UseSeqLogging = context.IsDevelopment();
                }
            )
            .ConfigureSingleInstance(options =>
                {
                    options.MutexId = $"{{{Constants.InstanceId}}}";
                    options.WhenNotFirstInstance = (environment, logger) =>
                    {
                        logger.LogCritical("{AppName} is already running", environment.ApplicationName);
                    };
                }
            )
            .ConfigureServices((context, services) => services.AddAllDesktopHostServices(context.Configuration))
            .ConfigureWpf(options =>
                {
                    options.UseApplication<App>();

                    // Windows will be registered as singletons
                    options.UseWindow<OverlayWindow>();
                }
            )
            .UseWpfLifetime()
            .UseConsoleLifetime();

        try
        {
            var host = hostBuilder.Build();
            await host.MigrateDatabaseAsync<OverlayDbContext>().ConfigureAwait(false);
            await host.RunAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            await Console.Error.WriteLineAsync($"An error occurred during app startup: {ex.Message}");
            throw;
        }
    }

    private static async Task HandleInstallationBehaviourAsync(string[] args)
    {
        var launchHostBuilder = Host.CreateDefaultBuilder(args)
            .UseCommonServices((context, options) =>
                {
                    options.UseFileLogging = true;
                    options.UseSeqLogging = context.IsDevelopment();
                }
            )
            .ConfigureServices((_, services) => services
                .AddVelopackServices()
                .AddFakeAnalyticsServices()
                .AddSingleton<IStorageManager, StorageManager>()
                .AddSingleton<ISystemAutoStartStateProvider, NoSystemAutoStartStateProvider>()
                .AddSingleton<ISchedulerFactory, FakeSchedulerFactory>()
                .AddSingleton<WindowsNotifications>()
                .AddServicesForUserPreferencesFromJsonFile()
            );

        using var launchApp = launchHostBuilder.Build();
        using var loggerFactory = launchApp.Services.GetRequiredService<ILoggerFactory>();

        var preferencesManager = launchApp.Services.GetRequiredService<IUserPreferencesManager>();
        await preferencesManager.LoadUserPreferencesAsync();

        var logger = loggerFactory.CreateLogger(typeof(Program));
        logger.LogDebug("Running velopack with args: '{Args}'", string.Join("', '", args));

        var userPreferences = preferencesManager.CurrentPreferences;
        VelopackApp.Build()
            .SetArgs(args)
            .OnFirstRun(_ => WindowsNotifications.ShowWelcomeToast(userPreferences))
            .OnAfterUpdateFastCallback(WindowsNotifications.ShowUpdatedToast)
            .Run();

        try
        {
            logger.LogDebug("Starting update process for channel: {UpdateChannel}", userPreferences.UpdateChannel);
            using var update = ActivatorUtilities.CreateInstance<UpdateProcess>(launchApp.Services);
            await update.RunAsync(true, CancellationToken.None);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Update process failed for channel: {UpdateChannel}", userPreferences.UpdateChannel);
        }
    }

    internal static IServiceCollection AddAllDesktopHostServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureConfiguration(configuration);

        services.AddWpfBlazorWebView();
        services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.MaxDisplayedSnackbars = 1;
            }
        );
        services.AddSingleton<IServiceProvider>(sp => sp);
        services.AddHttpClient();

        services.AddGoogleTrackingServices()
            .AddSharedComponentServices()
            .AddSingleton<SharedAnalyticsPropertyProvider, DesktopAnalyticsPropertyProvider>();

        services.AddGlobalKeyboardProxyService();
        services.AddJavaScriptEventInterop();
        services.AddSingleton(typeof(WindowProvider<>));

        services.AddHostedService<WindowsAutoStartManager>()
            .AddSingleton<ISystemAutoStartStateProvider, WindowsAutoStartStateProvider>();

        services.AddSingleton<WindowsNotifications>();

        // Auto updater
        services.AddVelopackServices();

        // Data
        services
            .AddWindowsOverlayControls()
            .AddInfrastructure(options => options.HostingMode = HostingMode.LocalSingleUser);

        // Singleton Services
        services.AddSingleton<BlurHelper>();
        services.AddSingleton(typeof(WindowControls<>));
        services.AddMemoryCache();

        // Factories
        services.AddSingleton<WindowFactory>();

        // Workers
        services.AddSingleton<WindowTracker>()
            .Alias<IHostedService, WindowTracker>();

        services.AddSingleton<GlobalHotkey>()
            .Alias<IHostedService, GlobalHotkey>();

        return services;
    }

    internal static IServiceCollection AddVelopackServices(this IServiceCollection services)
        => services
            .AddTransient<IUpdateSource>(provider =>
                {
                    var userPreferencesProvider = provider.GetRequiredService<IUserPreferencesProvider>();
                    return UpdateHelper.CreateSourceFor(userPreferencesProvider.CurrentPreferences.UpdateChannel);
                }
            )
            .AddTransient<UpdateOptions>(provider => new UpdateOptions
                {
                    AllowVersionDowngrade = true,
                    ExplicitChannel = provider.GetRequiredService<IUserPreferencesProvider>().CurrentPreferences.UpdateChannel.VelopackChannelId,
                }
            )
            .AddTransient<ArkanisOverlayUpdateManager>(provider => ActivatorUtilities.CreateInstance<ArkanisOverlayUpdateManager>(provider))
            .AddTransient<IAppVersionProvider, VelopackAppVersionProvider>()
            .AddHostedService<UpdateProcess.CheckForUpdatesJob.SelfScheduleService>();
}
