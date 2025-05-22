namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Trade;

public class FakePriceProvider : IPriceProvider
{
    public ValueTask UpdatePriceTagAsync(IGamePurchasable gameEntity)
        => ValueTask.CompletedTask;

    public ValueTask<ICollection<PriceTag>> GetPriceTagsWithinAsync(IGamePurchasable gameEntity, IGameLocation? gameLocation)
        => ValueTask.FromResult<ICollection<PriceTag>>([PriceTag.Unknown]);

    public ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGamePurchasable gameEntity, IGameLocation gameLocation)
        => ValueTask.FromResult(Bounds.All(PriceTag.MissingFor(gameLocation)));

    public ValueTask UpdatePriceTagAsync(IGameSellable gameEntity)
        => ValueTask.CompletedTask;

    public ValueTask<ICollection<PriceTag>> GetPriceTagsWithinAsync(IGameSellable gameEntity, IGameLocation? gameLocation)
        => ValueTask.FromResult<ICollection<PriceTag>>([PriceTag.Unknown]);

    public ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGameSellable gameEntity, IGameLocation gameLocation)
        => ValueTask.FromResult(Bounds.All(PriceTag.Unknown));

    public ValueTask UpdatePriceTagAsync(IGameRentable gameEntity)
        => ValueTask.CompletedTask;

    public ValueTask<ICollection<PriceTag>> GetPriceTagsWithinAsync(IGameRentable gameEntity, IGameLocation? gameLocation)
        => ValueTask.FromResult<ICollection<PriceTag>>([PriceTag.Unknown]);

    public ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGameRentable gameEntity, IGameLocation gameLocation)
        => ValueTask.FromResult(Bounds.All(PriceTag.Unknown));

    public bool IsReady
        => true;

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public ValueTask<Bounds<PriceTag>> GetMarketPriceTagAsync(IGameEntity gameEntity)
        => ValueTask.FromResult(Bounds.All(PriceTag.Unknown));
}
