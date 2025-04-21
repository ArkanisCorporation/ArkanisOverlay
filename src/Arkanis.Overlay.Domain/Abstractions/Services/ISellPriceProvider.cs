namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models.Trade;

public interface ISellPriceProvider : IDependable
{
    ValueTask UpdatePriceTagAsync(IGameSellable gameEntity);

    ValueTask<PriceTag> GetPriceTagAtAsync(IGameSellable gameEntity, IGameLocation gameLocation);
}
