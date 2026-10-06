namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Attributes;
using Enums;
using Search;

[Description("Game Company Entry")]
[CacheEntryDescription("Game Companies")]
public sealed class GameCompany(int id, string fullName, string shortName)
    : GameEntity(UexApiGameEntityId.Create<GameCompany>(id), GameEntityCategory.Company)
{
    public static readonly GameCompany Unknown = new(0, "Unknown Company", "Unknown");

    public override GameEntityName Name { get; } = new(new GameEntityName.NameWithShortVariant(fullName, shortName));

    protected override IEnumerable<SearchableTrait> CollectSearchableTraits()
    {
        yield return new SearchableName(fullName);
        foreach (var searchableAttribute in base.CollectSearchableTraits())
        {
            yield return searchableAttribute;
        }
    }
}
