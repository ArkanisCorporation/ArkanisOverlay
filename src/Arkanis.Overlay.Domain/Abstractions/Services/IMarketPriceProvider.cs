namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models;
using Models.Trade;

public interface IMarketPriceProvider : IDependable
{
    public ValueTask<Bounds<PriceTag>> GetMarketPriceTagAsync(IGameEntity gameEntity);
}
