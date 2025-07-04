namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Attributes;
using Search;

[Description("Game Outpost Entry")]
[CacheEntryDescription("Game Outposts")]
public sealed class GameOutpost(int id, string fullName, string shortName, GameLocationEntity location)
    : GameLocationEntity(UexApiGameEntityId.Create<GameOutpost>(id), location)
{
    public override GameEntityName Name { get; } = new(
        GameEntityName.ReferenceTo(location),
        new GameEntityName.NameWithShortVariant(fullName, shortName)
    );

    protected override IEnumerable<SearchableTrait> CollectSearchableTraits()
    {
        yield return new SearchableName(fullName);
        foreach (var searchableAttribute in base.CollectSearchableTraits())
        {
            yield return searchableAttribute;
        }
    }
}
