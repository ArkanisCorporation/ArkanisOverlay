namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models;
using Models.Trade;

public interface IRentPriceProvider : IDependable
{
    public ValueTask UpdatePriceTagAsync(IGameRentable gameEntity);

    public ValueTask<ICollection<PriceTag>> GetPriceTagsWithinAsync(IGameRentable gameEntity, IGameLocation? gameLocation);

    public ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGameRentable gameEntity, IGameLocation gameLocation);
}
