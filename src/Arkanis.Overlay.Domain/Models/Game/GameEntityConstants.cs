namespace Arkanis.Overlay.Domain.Models.Game;

using Abstractions.Game;

public static class GameEntityConstants
{
    public static readonly Type[] GameEntityTypes
        = [.. typeof(GameEntityConstants).Assembly
            .GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IGameEntity)))
            .Where(type => !type.IsAbstract)
            .Where(type => type.IsPublic)];
}
