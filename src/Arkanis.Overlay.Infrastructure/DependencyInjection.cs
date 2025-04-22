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

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        => services
            .AddSingleton<ServiceDependencyResolver>()
            .AddHostedService<InitializeServicesHostedService>()
            .AddAllUexApiClients()
            .AddUexDatabaseServices()
            .AddInMemorySearchServices()
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
