namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Enums;
using Models;
using Models.Trade;

public interface IGamePurchasable : IGameEntity
{
    Bounds<PriceTag> LatestPurchasePrices { get; }
    GameTerminalType TerminalType { get; }

    void UpdatePurchasePrices(Bounds<PriceTag> newPrices);
}
