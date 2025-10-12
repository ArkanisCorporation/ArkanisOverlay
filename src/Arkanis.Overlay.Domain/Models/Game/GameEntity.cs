namespace Arkanis.Overlay.Domain.Models.Game;

using Abstractions.Game;
using Enums;
using Search;

public abstract class GameEntity(UexApiGameEntityId id, GameEntityCategory entityCategory) : IGameEntity
{
    private SearchableTrait[]? _searchableTraits;

    public IEnumerable<SearchableTrait> SearchableAttributes
        => _searchableTraits ??= [.. CollectSearchableTraits()];

    public GameEntityCategory EntityCategory { get; } = entityCategory;

    public abstract GameEntityName Name { get; }

    public UexApiGameEntityId Id { get; } = id;

    protected virtual IEnumerable<SearchableTrait> CollectSearchableTraits()
    {
        yield return new SearchableEntityCategory(EntityCategory);
    }
}
