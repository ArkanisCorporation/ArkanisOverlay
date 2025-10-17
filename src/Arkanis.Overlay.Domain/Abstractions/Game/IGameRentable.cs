namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Enums;
using Models;
using Models.Trade;

public interface IGameRentable : IGameEntity
{
    public Bounds<PriceTag> LatestRentPrices { get; }
    public GameTerminalType TerminalType { get; }

    public void UpdateRentPrices(Bounds<PriceTag> newPrices);
}
