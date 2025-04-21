namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Enums;
using Models;
using Models.Trade;

public interface IGameRentable : IGameEntity
{
    Bounds<PriceTag> LatestRentPrices { get; }
    GameTerminalType TerminalType { get; }

    void UpdateRentPrices(Bounds<PriceTag> newPrices);
}
