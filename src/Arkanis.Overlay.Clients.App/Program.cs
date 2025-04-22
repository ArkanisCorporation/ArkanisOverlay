namespace Arkanis.Overlay.Application;

using Dapplo.Microsoft.Extensions.Hosting.AppServices;
using Dapplo.Microsoft.Extensions.Hosting.Wpf;
using External.UEX;
using Helpers;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.Extensions;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Services;
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
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging()
            .ConfigureSingleInstance
            (
                options =>
                {
                    options.MutexId = $"{{{Constants.InstanceId}}}";
                    options.WhenNotFirstInstance = (environment, logger) =>
                    {
                        logger.LogInformation("{appName} is already running.", environment.ApplicationName);
                    };
                }
            )
            //? add plugin support later to support modular add-ons?
            // .ConfigurePlugins()
            .ConfigureServices()
            .ConfigureWpf
            (
                options =>
                {
                    options.UseApplication<App>();

                    //* Add Singleton Windows here
                    // Windows will be registered as singletons
                    options.UseWindow<OverlayWindow>();
                }
            )
            .UseWpfLifetime()
            .UseConsoleLifetime()
            .Build();

        await host.MigrateDatabaseAsync<UEXContext>().ConfigureAwait(false);
        await host.RunAsync().ConfigureAwait(false);
    }

    private static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder)
        => hostBuilder.ConfigureLogging
        (
            (hostContext, configLogging) =>
                configLogging
                    .AddConfiguration(hostContext.Configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddFilter
                    (
                        (scope, _)
                            => scope?.StartsWith("Arkanis") ?? false
                    )
        );

    private static IHostBuilder ConfigureServices(this IHostBuilder builder)
        => builder.ConfigureServices
        (
            (context, services) =>
            {
                services.AddInfrastructureConfiguration(context.Configuration);

                services.AddWpfBlazorWebView();
                services.AddMudServices
                (
                    config =>
                    {
                        config.SnackbarConfiguration.NewestOnTop = true;
                        config.SnackbarConfiguration.MaxDisplayedSnackbars = 1;
                    }
                );
                services.AddSingleton<IServiceProvider>(sp => sp);
                services.AddHttpClient();

                services.AddAllExternalUexApiClients();

                // Data
                services
                    .AddWindowOverlayControls()
                    .AddInfrastructure()
                    .AddInfrastructureHostedServices();

                // Hosted Services
                services.AddHostedService<EndpointManager>();

                // Singleton Services
                services.AddSingleton<BlurHelper>();
                services.AddMemoryCache();

                // Workers
                services.AddSingleton<WindowTracker>();
                services.AddSingleton<GlobalHotkey>();
                services.AddSingleton<DataSync>();
            }
        );
}
