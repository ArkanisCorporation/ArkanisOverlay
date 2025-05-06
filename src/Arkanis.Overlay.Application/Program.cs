namespace Arkanis.Overlay.Application;

using Common.Extensions;
using System.Data.Common;
using Common;
using Components.Helpers;
using Components.Services;
using Dapplo.Microsoft.Extensions.Hosting.AppServices;
using Dapplo.Microsoft.Extensions.Hosting.Wpf;
using Helpers;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.Extensions;
using Infrastructure.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Services;
using Services.Factories;
using UI;
using UI.Windows;
using Workers;

// based on:
// https://github.com/dapplo/Dapplo.Microsoft.Extensions.Hosting/blob/master/samples/Dapplo.Hosting.Sample.WpfDemo/Program.cs#L48
public static class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureLogging()
            .ConfigureSingleInstance(options =>
                {
                    options.MutexId = $"{{{Constants.InstanceId}}}";
                    options.WhenNotFirstInstance = (environment, logger) =>
                    {
                        logger.LogInformation("{AppName} is already running", environment.ApplicationName);
                    };
                }
            )
            //? add plugin support later to support modular add-ons?
            // .ConfigurePlugins()
            .ConfigureServices()
            .ConfigureWpf(options =>
                {
                    options.UseApplication<App>();

                    //* Add Singleton Windows here
                    // Windows will be registered as singletons
                    options.UseWindow<OverlayWindow>();
                }
            )
            .UseWpfLifetime()
            .UseConsoleLifetime();

        try
        {
            ApplicationConstants.LocalAppDataDir.Create();
            var host = hostBuilder.Build();

            try
            {
                await host.MigrateDatabaseAsync<OverlayDbContext>().ConfigureAwait(false);
            }
            catch (DbException ex)
            {
                await Console.Error.WriteLineAsync($"Encountered a database error during migration: {ex.Message}");
                await Console.Error.WriteLineAsync($"Trying auto-recovery by deleting default appdata directory at {ApplicationConstants.LocalAppDataDir}");

                ApplicationConstants.LocalAppDataDir.Delete(true);
                ApplicationConstants.LocalAppDataDir.Create();
                await host.MigrateDatabaseAsync<OverlayDbContext>().ConfigureAwait(false);
            }

            await host.RunAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"An error occurred during app startup: {ex.Message}");
            throw;
        }
    }

    private static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder)
        => hostBuilder.ConfigureLogging((hostContext, configLogging) =>
            configLogging
                .AddConfiguration(hostContext.Configuration.GetSection("Logging"))
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Debug)
                .AddFilter((scope, _)
                    => scope?.StartsWith("Arkanis") ?? false
                )
        );

    private static IHostBuilder ConfigureServices(this IHostBuilder builder)
        => builder.ConfigureServices((context, services) =>
            {
                services.AddInfrastructureConfiguration(context.Configuration);

                services.AddWpfBlazorWebView();
                services.AddMudServices(config =>
                    {
                        config.SnackbarConfiguration.NewestOnTop = true;
                        config.SnackbarConfiguration.MaxDisplayedSnackbars = 1;
                    }
                );
                services.AddSingleton<IServiceProvider>(sp => sp);
                services.AddHttpClient();

                services.AddGlobalKeyboardProxyService();
                services.AddJavaScriptEventInterop();
                services.AddGoogleTrackingServices();
                services.AddSingleton(typeof(WindowProvider<>));

                services.AddHostedService<WindowsAutoStartManager>()
                    .AddSingleton<ISystemAutoStartStateProvider, WindowsAutoStartStateProvider>();

                // Data
                services
                    .AddWindowsOverlayControls()
                    .AddPreferenceServiceCollection()
                    .AddInfrastructure();

                // Singleton Services
                services.AddSingleton<BlurHelper>();
                services.AddMemoryCache();

                // Factories
                services.AddSingleton<PreferencesWindowFactory>();

                // Workers
                services.AddSingleton<WindowTracker>()
                    .Alias<IHostedService, WindowTracker>();

                services.AddSingleton<GlobalHotkey>()
                    .Alias<IHostedService, GlobalHotkey>();
            }
        );
}
