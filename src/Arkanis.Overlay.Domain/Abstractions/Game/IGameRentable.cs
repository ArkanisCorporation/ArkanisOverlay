namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Enums;
using Models;
using Models.Trade;

public interface IGameRentable
{
    Bounds<PriceTag> LatestRentPrices { get; }
    GameTerminalType TerminalType { get; }
}
