namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Enums;

public interface IGameTradable : IGamePurchasable, IGameSellable
{
    new GameTerminalType TerminalType { get; }

    GameTerminalType IGamePurchasable.TerminalType
        => TerminalType;

    GameTerminalType IGameSellable.TerminalType
        => TerminalType;
}
