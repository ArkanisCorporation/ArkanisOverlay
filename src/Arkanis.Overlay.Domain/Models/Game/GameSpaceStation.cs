namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Attributes;
using Search;

[Description("Game Space Station Entry")]
[CacheEntryDescription("Game Space Stations")]
public sealed class GameSpaceStation(int id, string fullName, string shortName, GameLocationEntity location)
    : GameLocationEntity(UexApiGameEntityId.Create<GameSpaceStation>(id), location)
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
