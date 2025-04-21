namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models.Trade;

public interface IRentPriceProvider : IDependable
{
    ValueTask UpdatePriceTagAsync(IGameRentable gameEntity);

    ValueTask<PriceTag> GetPriceTagAtAsync(IGameRentable gameEntity, IGameLocation gameLocation);
}
