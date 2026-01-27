namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Models.Game;

public interface IGameLocation : IGameEntity, ISearchableRecursively
{
    public HashSet<UexApiGameEntityId> ParentIds { get; }

    public IEnumerable<IGameLocation> Parents { get; }

    public IGameLocation? ParentLocation { get; }

    public string? ImageUrl { get; }

    public string? ImageAuthor { get; }

    ISearchableRecursively? ISearchableRecursively.Parent
        => ParentLocation;

    public IEnumerable<GameLocationEntity> CreatePathToRoot();

    public bool IsOrContains(IGameLocation location)
        => Id == location.Id || Contains(location);

    public bool Contains(IGameLocation location)
        => location.ParentIds.Contains(Id);
}
