namespace Arkanis.Overlay.Host.Server;

using Common.Abstractions;
using Common.Enums;
using Common.Services;
using Infrastructure;
using Infrastructure.Services;
using Infrastructure.Services.Abstractions;
using MudBlazor;
using MudBlazor.Services;
using Overlay.Components.Helpers;
using Overlay.Components.Services;
using Serilog;
using Services;

public static class DependencyInjection
{
    public static IServiceCollection AddAllServerHostServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog(loggerConfig => loggerConfig
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(configuration)
        );

        services.AddLogging();
        services.AddHttpClient();
        services.AddHealthChecks();

        services.AddMemoryCache();

        services
            .AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddMudServices(options =>
            {
                options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
                options.SnackbarConfiguration.PreventDuplicates = false;
                options.SnackbarConfiguration.ClearAfterNavigation = false;
            }
        );

        services
            .AddJavaScriptEventInterop()
            .AddGlobalKeyboardProxyService()
            .AddGoogleTrackingServices()
            .AddSharedComponentServices()
            .AddSingleton<SharedAnalyticsPropertyProvider, ServerAnalyticsPropertyProvider>()
            .AddServerOverlayControls()
            .AddInfrastructure(options => options.HostingMode = HostingMode.Server)
            .AddInfrastructureConfiguration(configuration)
            .AddSingleton<GitHubReleasesService>()
            .AddSingleton<IAppVersionProvider, AssemblyAppVersionProvider>()
            .AddSingleton<ISystemAutoStartStateProvider, NoSystemAutoStartStateProvider>();

        return services;
    }
}
