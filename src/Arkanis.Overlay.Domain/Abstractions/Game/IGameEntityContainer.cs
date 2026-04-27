namespace Arkanis.Overlay.Domain.Abstractions.Game;

/// <summary>
///     Signifies that the entity is fundamentally a part of another entity.
/// </summary>
public interface IGameEntityPart : IGameEntity
{
    public IGameEntity Owner { get; }
}

/// <summary>
///     Signifies that the entity is a container for other entity parts.
///     Prepared for vehicle components.
/// </summary>
public interface IGameEntityContainer : IGameEntity
{
    public IEnumerable<IGameEntityPart> Parts { get; }
}

/// <inheritdoc cref="IGameEntityContainer" />
public interface IGameEntityContainer<out T> : IGameEntityContainer
    where T : IGameEntityPart
{
    public new IEnumerable<T> Parts { get; }

    IEnumerable<IGameEntityPart> IGameEntityContainer.Parts
        => Parts.OfType<IGameEntityPart>();
}
