namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Attributes;
using Enums;
using Search;

/// <summary>
///     This is an item attribute in the context of the UEX API.
///     The "Trait" suffix was selected to differentiate it from the conventional "Attribute" class suffix in C#.
/// </summary>
/// <param name="id">Attribute ID</param>
/// <param name="itemId">Referenced item ID</param>
/// <param name="fullName">Name of the trait</param>
[Description("Game Item Trait Entry")]
[CacheEntryDescription("Game Item Traits")]
public class GameItemTrait(int id, int itemId, string fullName)
    : GameEntity(UexApiGameEntityId.Create<GameItemTrait>(id), GameEntityCategory.ItemTrait)
{
    public UexId<GameItem> ItemId { get; } = UexApiGameEntityId.Create<GameItem>(itemId);

    public required string Content { get; init; }

    public GameItemTraitType TraitType { get; init; } = fullName switch
    {
        "Grade" => GameItemTraitType.Grade,
        "Class" or "Armor Class" => GameItemTraitType.Class,
        _ => GameItemTraitType.Unspecified,
    };

    public override GameEntityName Name
        => new(new GameEntityName.Name(fullName));

    protected override IEnumerable<SearchableTrait> CollectSearchableTraits()
    {
        yield return new SearchableName($"{Name} {Content}");
        foreach (var searchableAttribute in base.CollectSearchableTraits())
        {
            yield return searchableAttribute;
        }
    }

    public GameEntityName.PropertyItem ToNamePart()
        => new(fullName, Content);
}
