namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Trade;

public class PriceProviderAggregate(
    ServiceDependencyResolver serviceDependencyResolver,
    IPurchasePriceProvider purchasePriceProvider,
    ISalePriceProvider salePriceProvider,
    IRentPriceProvider rentPriceProvider
) : IPriceProvider
{
    public ValueTask UpdatePriceTagAsync(IGamePurchasable gameEntity)
        => purchasePriceProvider.UpdatePriceTagAsync(gameEntity);

    public ValueTask UpdatePriceTagAsync(IGameSellable gameEntity)
        => salePriceProvider.UpdatePriceTagAsync(gameEntity);

    public ValueTask UpdatePriceTagAsync(IGameRentable gameEntity)
        => rentPriceProvider.UpdatePriceTagAsync(gameEntity);

    public bool IsReady { get; private set; }

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await serviceDependencyResolver.DependsOn(this, purchasePriceProvider, salePriceProvider, rentPriceProvider)
            .WaitUntilReadyAsync(cancellationToken)
            .ContinueWith(_ => IsReady = true, cancellationToken)
            .ConfigureAwait(false);

    public ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGamePurchasable gameEntity, IGameLocation gameLocation)
        => purchasePriceProvider.GetPriceTagAtAsync(gameEntity, gameLocation);

    public ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGameSellable gameEntity, IGameLocation gameLocation)
        => salePriceProvider.GetPriceTagAtAsync(gameEntity, gameLocation);

    public ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGameRentable gameEntity, IGameLocation gameLocation)
        => rentPriceProvider.GetPriceTagAtAsync(gameEntity, gameLocation);
}
