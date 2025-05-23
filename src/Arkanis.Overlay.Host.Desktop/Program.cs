namespace Arkanis.Overlay.Host.Desktop;

using System.Globalization;
using System.IO;
using Common;
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
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        await HandleInstallationBehaviourAsync(args);

        var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureSingleInstance(options =>
                {
                    options.MutexId = $"{{{Constants.InstanceId}}}";
                    options.WhenNotFirstInstance = (environment, logger) =>
                    {
                        logger.LogInformation("{AppName} is already running", environment.ApplicationName);
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
            .ConfigureServices((context, services) => services
                .AddLoggerServices(context.Configuration)
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
        services.AddLoggerServices(configuration);

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
            .AddSingleton<IUpdateSource>(provider =>
                {
                    var userPreferencesProvider = provider.GetRequiredService<IUserPreferencesProvider>();
                    return UpdateHelper.CreateSourceFor(userPreferencesProvider.CurrentPreferences.UpdateChannel);
                }
            )
            .AddSingleton<UpdateOptions>(provider => new UpdateOptions
                {
                    AllowVersionDowngrade = true,
                    ExplicitChannel = provider.GetRequiredService<IUserPreferencesProvider>().CurrentPreferences.UpdateChannel.VelopackChannelId,
                }
            )
            .AddSingleton<ArkanisOverlayUpdateManager>(provider => ActivatorUtilities.CreateInstance<ArkanisOverlayUpdateManager>(provider))
            .AddSingleton<IAppVersionProvider, VelopackAppVersionProvider>()
            .AddHostedService<UpdateProcess.CheckForUpdatesJob.SelfScheduleService>();

    internal static IServiceCollection AddLoggerServices(this IServiceCollection services, IConfiguration configuration)
        => services.AddSerilog(loggerConfig => loggerConfig
            .WriteTo.Console(
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}) {Message:lj}{NewLine}{Exception}",
                formatProvider: CultureInfo.InvariantCulture
            )
            .WriteTo.File(
                Path.Join(ApplicationConstants.ApplicationLogsDirectory.FullName, "app.log"),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}) {Message:lj}{NewLine}{Exception}",
                buffered: true,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                formatProvider: CultureInfo.InvariantCulture
            )
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(configuration)
        );
}
