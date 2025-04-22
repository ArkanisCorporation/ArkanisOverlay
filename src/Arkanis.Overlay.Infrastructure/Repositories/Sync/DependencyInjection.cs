namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Models.Game;
using Local;
using Microsoft.Extensions.DependencyInjection;
using Services;
using Services.Abstractions;

public static class DependencyInjection
{
    private static readonly Type ReferenceSourceType = typeof(DependencyInjection);

    internal static IServiceCollection AddUexSyncRepositoryServices(this IServiceCollection services)
    {
        services
            .AddUexApiMappers()
            .AddSingleton<UexGameDataStateProvider>()
            .AddSingleton<GameEntityRepositoryDependencyResolver>()
            .Scan(scan => scan
                .FromAssembliesOf(ReferenceSourceType)
                .AddClasses(type => type.InNamespaceOf(ReferenceSourceType), false)
                .AsImplementedInterfaces()
            );

        var syncServiceManagerType = typeof(GameEntityRepositorySyncManager<>);
        foreach (var gameEntityType in GameEntityConstants.GameEntityTypes)
        {
            var serviceType = syncServiceManagerType.MakeGenericType(gameEntityType);
            services.AddSingleton(serviceType);
            services.AddSingleton(typeof(ISelfInitializable), provider => provider.GetRequiredService(serviceType));
            services.AddSingleton(typeof(ISelfUpdatable), provider => provider.GetRequiredService(serviceType));
        }

        return services;
    }
}
