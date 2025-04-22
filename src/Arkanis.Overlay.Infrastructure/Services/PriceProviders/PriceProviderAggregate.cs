namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Trade;

public class PriceProviderAggregate(
    ServiceDependencyResolver serviceDependencyResolver,
    IPurchasePriceProvider purchasePriceProvider,
    ISellPriceProvider sellPriceProvider,
    IRentPriceProvider rentPriceProvider
) : IPriceProvider
{
    public ValueTask UpdatePriceTagAsync(IGamePurchasable gameEntity)
        => purchasePriceProvider.UpdatePriceTagAsync(gameEntity);

    public ValueTask<PriceTag> GetPriceTagAtAsync(IGamePurchasable gameEntity, IGameLocation gameLocation)
        => purchasePriceProvider.GetPriceTagAtAsync(gameEntity, gameLocation);

    public ValueTask UpdatePriceTagAsync(IGameSellable gameEntity)
        => sellPriceProvider.UpdatePriceTagAsync(gameEntity);

    public ValueTask<PriceTag> GetPriceTagAtAsync(IGameSellable gameEntity, IGameLocation gameLocation)
        => sellPriceProvider.GetPriceTagAtAsync(gameEntity, gameLocation);

    public ValueTask UpdatePriceTagAsync(IGameRentable gameEntity)
        => rentPriceProvider.UpdatePriceTagAsync(gameEntity);

    public ValueTask<PriceTag> GetPriceTagAtAsync(IGameRentable gameEntity, IGameLocation gameLocation)
        => rentPriceProvider.GetPriceTagAtAsync(gameEntity, gameLocation);

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await serviceDependencyResolver.DependsOn(this, purchasePriceProvider, sellPriceProvider, rentPriceProvider)
            .WaitUntilReadyAsync(cancellationToken)
            .ConfigureAwait(false);
}
