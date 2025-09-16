namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Common.Abstractions.Services;
using Game;
using Models;
using Models.Trade;

public interface IMarketPriceProvider : IDependable
{
    ValueTask<Bounds<PriceTag>> GetMarketPriceTagAsync(IGameEntity gameEntity);
}
