namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Attributes;
using Search;

[Description("Game Planet Entry")]
[CacheEntryDescription("Game Planets")]
public sealed class GamePlanet(int id, string fullName, string codeName, GameLocationEntity location)
    : GameLocationEntity(UexApiGameEntityId.Create<GamePlanet>(id), location)
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
