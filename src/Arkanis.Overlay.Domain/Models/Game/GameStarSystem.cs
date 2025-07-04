namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Attributes;
using Search;

[Description("Game Star System Entry")]
[CacheEntryDescription("Game Star Systems")]
public sealed class GameStarSystem(int id, string fullName, string codeName)
    : GameLocationEntity(UexApiGameEntityId.Create<GameStarSystem>(id), null)
{
    public override GameEntityName Name { get; } = new(new GameEntityName.NameWithCode(fullName, codeName));

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
