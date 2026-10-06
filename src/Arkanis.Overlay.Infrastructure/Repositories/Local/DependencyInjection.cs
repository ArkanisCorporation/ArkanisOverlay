namespace Arkanis.Overlay.Infrastructure.Repositories.Local;

using Common.Extensions;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using Microsoft.Extensions.DependencyInjection;
using Specialised;

public static class DependencyInjection
{
    internal static IServiceCollection AddUexInMemoryRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IGameEntityAggregateRepository, GameEntitySearchAggregateRepository>();

        var genericRepositoryInterfaceType = typeof(IGameEntityRepository<>);
        var genericRepositoryServiceType = typeof(UexGameEntityInMemoryRepository<>);
        var genericRepositoryAdapterType = typeof(UexGameEntityGenericRepositoryAdapter<>);
        foreach (var gameEntityType in GameEntityConstants.GameEntityTypes)
        {
            var repositoryInterfaceType = genericRepositoryInterfaceType.MakeGenericType(gameEntityType);
            var repositoryType = genericRepositoryServiceType.MakeGenericType(gameEntityType);
            services.AddSingleton(repositoryInterfaceType, repositoryType);

            var adapterType = genericRepositoryAdapterType.MakeGenericType(gameEntityType);
            services.AddSingleton(typeof(IGameEntityRepository), adapterType);
        }

        services.AddSingleton<GameMarketPricingRepositoryAggregate>()
            .AddSingleton<GamePurchasePricingRepositoryAggregate>()
            .AddSingleton<GameSalePricingRepositoryAggregate>()
            .AddSingleton<GameRentalPricingRepositoryAggregate>();

        services
            .Decorate<IGameEntityRepository<GameItemTrait>>(inner => new UexGameItemTraitRepositorySpecialisationDecorator(inner))
            .AliasVia<IGameItemTraitRepository, IGameEntityRepository<GameItemTrait>, UexGameItemTraitRepositorySpecialisationDecorator>();

        services
            .Decorate<IGameEntityRepository<GameEntityPurchasePrice>>(inner => new UexPurchasePricingRepositorySpecialisationDecorator(inner))
            .AliasVia<IGamePurchasePricingRepository, IGameEntityRepository<GameEntityPurchasePrice>, UexPurchasePricingRepositorySpecialisationDecorator>();

        services
            .Decorate<IGameEntityRepository<GameEntitySalePrice>>(inner => new UexSalePricingRepositorySpecialisationDecorator(inner))
            .AliasVia<IGameSalePricingRepository, IGameEntityRepository<GameEntitySalePrice>, UexSalePricingRepositorySpecialisationDecorator>();

        services
            .Decorate<IGameEntityRepository<GameEntityRentalPrice>>(inner => new UexRentalPricingRepositorySpecialisationDecorator(inner))
            .AliasVia<IGameRentalPricingRepository, IGameEntityRepository<GameEntityRentalPrice>, UexRentalPricingRepositorySpecialisationDecorator>();

        services
            .Decorate<IGameEntityRepository<GameEntityMarketPrice>>(inner => new UexMarketPricingRepositorySpecialisationDecorator(inner))
            .AliasVia<IGameMarketPricingRepository, IGameEntityRepository<GameEntityMarketPrice>, UexMarketPricingRepositorySpecialisationDecorator>();

        // decorating each trade repository with purchase and sale repository interfaces
        //   then registering each decorated repository under the appropriate interface
        services
            .Decorate<IGameEntityRepository<GameEntityTradePrice>>(inner => new UexTradePricingRepositorySpecialisationDecorator(inner))
            .AliasVia<IGamePurchasePricingRepository, IGameEntityRepository<GameEntityTradePrice>, UexTradePricingRepositorySpecialisationDecorator>()
            .AliasVia<IGameSalePricingRepository, IGameEntityRepository<GameEntityTradePrice>, UexTradePricingRepositorySpecialisationDecorator>();

        return services;
    }
}
