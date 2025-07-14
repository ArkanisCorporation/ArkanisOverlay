namespace Arkanis.Overlay.Host.Server;

using System.Globalization;
using Common.Abstractions;
using Common.Abstractions.Services;
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
using Serilog.Templates;
using Serilog.Templates.Themes;
using Services;

public static class DependencyInjection
{
    public static IServiceCollection AddAllServerHostServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog(loggerConfig => loggerConfig
            .WriteTo.Console(
                new ExpressionTemplate(
                    "[{@t:HH:mm:ss} {@l:u3}] [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}] {@m}\n{@x}",
                    CultureInfo.InvariantCulture,
                    applyThemeWhenOutputIsRedirected: true,
                    theme: TemplateTheme.Literate
                )
            )
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
