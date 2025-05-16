namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models;
using Models.Trade;

public interface ISalePriceProvider : IDependable
{
    ValueTask UpdatePriceTagAsync(IGameSellable gameEntity);

    ValueTask<ICollection<PriceTag>> GetPriceTagsWithinAsync(IGameSellable gameEntity, IGameLocation? gameLocation);

    ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGameSellable gameEntity, IGameLocation gameLocation);
}
