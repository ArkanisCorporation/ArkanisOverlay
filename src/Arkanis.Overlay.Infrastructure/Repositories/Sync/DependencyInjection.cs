namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Common.Extensions;
using Data.Mappers;
using Domain.Abstractions.Services;
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
            .AddSingleton<GameEntityRepositoryDependencyResolver>()
            .Scan(scan => scan
                .FromAssembliesOf(ReferenceSourceType)
                .AddClasses(
                    type => type
                        .InNamespaceOf(ReferenceSourceType)
                        .Where(repositoryType => !repositoryType.IsAssignableTo(typeof(IDecoratorService))),
                    false
                )
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );

        services
            .AddSingleton<UexServiceStateProvider>()
            .Alias<IExternalServiceStateProvider, UexServiceStateProvider>();

        var syncServiceManagerType = typeof(GameEntityRepositorySyncManager<>);
        foreach (var gameEntityType in GameEntityConstants.GameEntityTypes)
        {
            var serviceType = syncServiceManagerType.MakeGenericType(gameEntityType);
            services.AddSingleton(serviceType);
            services.UnsafeAlias<ISelfInitializable>(serviceType);
            services.UnsafeAlias<ISelfUpdatable>(serviceType);
        }

        return services;
    }
}
