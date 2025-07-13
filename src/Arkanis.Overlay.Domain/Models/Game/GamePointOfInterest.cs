namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Attributes;
using Search;

[Description("Game Point of Interest Entry")]
[CacheEntryDescription("Game Points of Interest")]
public sealed class GamePointOfInterest(int id, string fullName, string shortName, GameLocationEntity location)
    : GameLocationEntity(UexApiGameEntityId.Create<GamePointOfInterest>(id), location)
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
