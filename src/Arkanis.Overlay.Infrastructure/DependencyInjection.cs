namespace Arkanis.Overlay.Infrastructure;

using Common.Extensions;
using Data;
using External.UEX;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using Repositories;
using Services;
using Services.Hosted;
using Services.Hydration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        => services
            .AddSingleton<ServiceDependencyResolver>()
            .AddHostedService<InitializeServicesHostedService>()
            .AddAllUexApiClients()
            .AddOverlaySqliteDatabaseServices()
            .AddInMemorySearchServices()
            .AddUexInMemoryGameEntityServices()
            .AddUserPreferencesFileManagerServices()
            .AddUexPriceProviders()
            .AddUexHydrationServices();

    public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddConfiguration<ConfigurationOptions>(configuration);
}
