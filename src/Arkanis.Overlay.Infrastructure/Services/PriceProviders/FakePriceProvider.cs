namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Trade;

public class FakePriceProvider : IPriceProvider
{
    public ValueTask UpdatePriceTagAsync(IGamePurchasable gameEntity)
        => ValueTask.CompletedTask;

    public ValueTask<PriceTag> GetPriceTagAtAsync(IGamePurchasable gameEntity, IGameLocation gameLocation)
        => ValueTask.FromResult(PriceTag.MissingFor(gameLocation));

    public ValueTask UpdatePriceTagAsync(IGameSellable gameEntity)
        => ValueTask.CompletedTask;

    public ValueTask<PriceTag> GetPriceTagAtAsync(IGameSellable gameEntity, IGameLocation gameLocation)
        => ValueTask.FromResult(PriceTag.Unknown);

    public ValueTask UpdatePriceTagAsync(IGameRentable gameEntity)
        => ValueTask.CompletedTask;

    public ValueTask<PriceTag> GetPriceTagAtAsync(IGameRentable gameEntity, IGameLocation gameLocation)
        => ValueTask.FromResult(PriceTag.Unknown);

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
