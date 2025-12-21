namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Enums;
using Models;
using Models.Trade;

public interface IGamePurchasable : IGameEntity
{
    public Bounds<PriceTag> LatestPurchasePrices { get; }
    public GameTerminalType TerminalType { get; }

    public void UpdatePurchasePrices(Bounds<PriceTag> newPrices);
}
