namespace Arkanis.Overlay.Infrastructure;

using Common.Extensions;
using Data;
using External.UEX;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using Repositories;
using Repositories.Sync;
using Services;
using Services.Hosted;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        => services
            .AddHostedService<InitializeServicesHostedService>()
            .AddAllUexApiClients()
            .AddUexDatabaseServices()
            .AddInMemorySearchServices()
            .AddUexSyncRepositoryServices()
            .AddUexInMemoryGameEntityServices()
            .AddUserPreferencesFileManagerServices()
            .AddGameEntityPriceHydratationServices();

    public static IServiceCollection AddInfrastructureHostedServices(this IServiceCollection services)
        => services
            .AddEndpointManagerHostedService();

    public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddConfiguration<ConfigurationOptions>(configuration);
}
