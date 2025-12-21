namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Enums;

public interface IGameTradable : IGamePurchasable, IGameSellable
{
    public new GameTerminalType TerminalType { get; }

    GameTerminalType IGamePurchasable.TerminalType
        => TerminalType;

    GameTerminalType IGameSellable.TerminalType
        => TerminalType;
}
