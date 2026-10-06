namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Attributes;
using Enums;
using Search;

[Description("Game Product Category Entry")]
[CacheEntryDescription("Game Product Categories")]
public sealed class GameProductCategory(int id, string fullName, string section)
    : GameEntity(UexApiGameEntityId.Create<GameProductCategory>(id), GameEntityCategory.ProductCategory)
{
    public static readonly GameProductCategory Unknown = new(0, "Unknown", string.Empty)
    {
        CategoryType = GameItemCategoryType.Undefined,
    };

    public override GameEntityName Name { get; } = new(new GameEntityName.Name($"{section} / {fullName}"));

    public required GameItemCategoryType CategoryType { get; set; }

    protected override IEnumerable<SearchableTrait> CollectSearchableTraits()
    {
        yield return new SearchableName(fullName);
        foreach (var searchableAttribute in base.CollectSearchableTraits())
        {
            yield return searchableAttribute;
        }
    }
}
