namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models;
using Models.Trade;

public interface ISalePriceProvider : IDependable
{
    public ValueTask UpdatePriceTagAsync(IGameSellable gameEntity);

    public ValueTask<ICollection<PriceTag>> GetPriceTagsWithinAsync(IGameSellable gameEntity, IGameLocation? gameLocation);

    public ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGameSellable gameEntity, IGameLocation gameLocation);
}
