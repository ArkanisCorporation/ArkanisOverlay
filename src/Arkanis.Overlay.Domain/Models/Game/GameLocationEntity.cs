namespace Arkanis.Overlay.Domain.Models.Game;

using Abstractions.Game;
using Enums;
using Search;

public abstract class GameLocationEntity(UexApiGameEntityId id, GameLocationEntity? parent)
    : GameEntity(id, GameEntityCategory.Location), IGameLocation
{
    public static readonly GameLocationEntity Unknown = new UnknownLocation();

    public GameLocationEntity? Parent { get; } = parent;

    public string? ImageUrl { get; set; }
    public string? ImageAuthor { get; set; }

    public HashSet<UexApiGameEntityId> ParentIds { get; } = parent is not null
        ? [parent.Id, ..parent.ParentIds]
        : [];

    public IEnumerable<IGameLocation> Parents { get; } = parent is not null
        ? [parent, ..parent.Parents]
        : [];

    IGameLocation? IGameLocation.ParentLocation
        => Parent;

    protected override IEnumerable<SearchableTrait> CollectSearchableTraits()
    {
        if (Parent is not null)
        {
            yield return new SearchableLocation(this);
        }

        foreach (var searchableAttribute in base.CollectSearchableTraits())
        {
            yield return searchableAttribute;
        }
    }

    public IEnumerable<GameLocationEntity> CreatePathToRoot()
    {
        foreach (var parent in Parent?.CreatePathToRoot() ?? [])
        {
            yield return parent;
        }

        yield return this;
    }

    private sealed class UnknownLocation() : GameLocationEntity(UexApiGameEntityId.Create<GameLocationEntity>(0), null)
    {
        public static GameLocationEntity Instance { get; } = new UnknownLocation();

        public override GameEntityName Name { get; } = new(new GameEntityName.NameWithCode("Unknown Location", "UNK?"));
    }
}
