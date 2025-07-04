namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Attributes;
using Search;

[Description("Game City Entry")]
[CacheEntryDescription("Game Cities")]
public sealed class GameCity(int id, string fullName, string codeName, GameLocationEntity location)
    : GameLocationEntity(UexApiGameEntityId.Create<GameCity>(id), location)
{
    public override GameEntityName Name { get; } = new(
        GameEntityName.ReferenceTo(location),
        new GameEntityName.NameWithCode(fullName, codeName)
    );

    protected override IEnumerable<SearchableTrait> CollectSearchableTraits()
    {
        yield return new SearchableName(fullName);
        yield return new SearchableCode(codeName);
        foreach (var searchableAttribute in base.CollectSearchableTraits())
        {
            yield return searchableAttribute;
        }
    }
}
