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

        services
            .Decorate<IGameEntityRepository<GameItemTrait>, UexGameItemTraitRepositorySpecialisationDecorator>()
            .AliasVia<IGameItemTraitRepository, IGameEntityRepository<GameItemTrait>, UexGameItemTraitRepositorySpecialisationDecorator>();

        services
            .Decorate<IGameEntityRepository<GameCommodityPricing>, UexCommodityPricingRepositorySpecialisationDecorator>()
            .AliasVia<IGameCommodityPricingRepository, IGameEntityRepository<GameCommodityPricing>, UexCommodityPricingRepositorySpecialisationDecorator>();

        services
            .Decorate<IGameEntityRepository<GameItemPurchasePricing>, UexItemPricingRepositorySpecialisationDecorator>()
            .AliasVia<IGameItemPurchasePricingRepository, IGameEntityRepository<GameItemPurchasePricing>, UexItemPricingRepositorySpecialisationDecorator>();

        services
            .Decorate<IGameEntityRepository<GameVehiclePurchasePricing>, UexVehiclePurchasePricingRepositorySpecialisationDecorator>()
            .AliasVia<IGameVehiclePurchasePricingRepository,
                IGameEntityRepository<GameVehiclePurchasePricing>,
                UexVehiclePurchasePricingRepositorySpecialisationDecorator>();

        services
            .Decorate<IGameEntityRepository<GameVehicleRentalPricing>, UexVehicleRentalPricingRepositorySpecialisationDecorator>()
            .AliasVia<IGameVehicleRentalPricingRepository,
                IGameEntityRepository<GameVehicleRentalPricing>,
                UexVehicleRentalPricingRepositorySpecialisationDecorator>();

        return services;
    }
}
