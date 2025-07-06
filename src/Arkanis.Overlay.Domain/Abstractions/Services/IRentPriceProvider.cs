namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Common.Abstractions.Services;
using Game;
using Models;
using Models.Trade;

public interface IRentPriceProvider : IDependable
{
    ValueTask UpdatePriceTagAsync(IGameRentable gameEntity);

    ValueTask<ICollection<PriceTag>> GetPriceTagsWithinAsync(IGameRentable gameEntity, IGameLocation? gameLocation);

    ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGameRentable gameEntity, IGameLocation gameLocation);
}
