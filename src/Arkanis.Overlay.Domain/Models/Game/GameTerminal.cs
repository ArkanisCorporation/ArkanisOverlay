namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Attributes;
using Enums;
using Search;

[Description("Game Terminal Entry")]
[CacheEntryDescription("Game Terminals")]
public sealed class GameTerminal(
    int id,
    string fullName,
    string shortName,
    string codeName,
    GameLocationEntity location
) : GameLocationEntity(UexApiGameEntityId.Create<GameTerminal>(id), location)
{
    public override GameEntityName Name { get; } = new(
        GameEntityName.ReferenceTo(location),
        new GameEntityName.NameWithCodeAndShortVariant(fullName, codeName, shortName)
    );

    public required GameTerminalType Type { get; init; }

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
