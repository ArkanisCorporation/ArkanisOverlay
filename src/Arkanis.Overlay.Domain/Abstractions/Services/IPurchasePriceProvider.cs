namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models;
using Models.Trade;

public interface IPurchasePriceProvider : IDependable
{
    public ValueTask UpdatePriceTagAsync(IGamePurchasable gameEntity);

    public ValueTask<ICollection<PriceTag>> GetPriceTagsWithinAsync(IGamePurchasable gameEntity, IGameLocation? gameLocation);

    public ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGamePurchasable gameEntity, IGameLocation gameLocation);
}
