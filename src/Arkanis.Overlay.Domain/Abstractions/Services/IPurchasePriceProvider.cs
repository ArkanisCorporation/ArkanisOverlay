namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models;
using Models.Trade;

public interface IPurchasePriceProvider : IDependable
{
    ValueTask UpdatePriceTagAsync(IGamePurchasable gameEntity);

    ValueTask<ICollection<PriceTag>> GetPriceTagsWithinAsync(IGamePurchasable gameEntity, IGameLocation? gameLocation);

    ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGamePurchasable gameEntity, IGameLocation gameLocation);
}
