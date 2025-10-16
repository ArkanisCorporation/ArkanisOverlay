namespace Arkanis.Overlay.External.UEX;

using Abstractions;
using Common.Extensions;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class DependencyInjection
{
    public static IServiceCollection AddAllUexApiClients(
        this IServiceCollection services,
        Func<IServiceProvider, IConfigureOptions<UexApiOptions>>? createOptions = null
    )
        => services
            .AddSingleton(createOptions ?? (_ => new ConfigureOptions<UexApiOptions>(_ => { })))
            .AddSingleton<IUexCrewApi, UexCrewApi>()
            .AddSingleton<IUexCommoditiesApi, UexCommoditiesApi>()
            .AddSingleton<IUexFuelApi, UexFuelApi>()
            .AddSingleton<IUexGameApi, UexGameApi>()
            .AddSingleton<IUexItemsApi, UexItemsApi>()
            .AddSingleton<IUexMarketplaceApi, UexMarketplaceApi>()
            .AddSingleton<IUexOrganizationsApi, UexOrganizationsApi>()
            .AddSingleton<IUexRefineriesApi, UexRefineriesApi>()
            .AddSingleton<IUexStaticApi, UexStaticApi>()
            .AddSingleton<IUexUserApi, UexUserApi>()
            .AddSingleton<IUexVehiclesApi, UexVehiclesApi>();

    public static IServiceCollection AddUexAuthenticatorServices(this IServiceCollection services)
        => services
            .AddSingleton<UexAuthenticator>()
            .Alias<ExternalAuthenticator, UexAuthenticator>();
}
