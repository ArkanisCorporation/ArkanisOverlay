namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Enums;
using Models;
using Models.Trade;

public interface IGameSellable : IGameEntity
{
    Bounds<PriceTag> LatestSellPrices { get; }
    GameTerminalType TerminalType { get; }

    void UpdateSellPrices(Bounds<PriceTag> newPrices);
}
