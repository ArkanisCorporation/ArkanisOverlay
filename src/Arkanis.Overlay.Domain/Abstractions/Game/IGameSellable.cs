namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Enums;
using Models;
using Models.Trade;

public interface IGameSellable : IGameEntity
{
    public Bounds<PriceTag> LatestSalePrices { get; }
    public GameTerminalType TerminalType { get; }

    public void UpdateSalePrices(Bounds<PriceTag> newPrices);
}
