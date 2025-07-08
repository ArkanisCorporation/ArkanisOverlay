namespace Arkanis.Overlay.Domain.Models.Game;

using System.Collections;
using System.ComponentModel;
using Abstractions.Game;
using Attributes;
using Enums;
using Search;
using Trade;

[Description("Game Item Entry")]
[CacheEntryDescription("Game Items")]
public class GameItem(int id, string fullName, GameCompany manufacturer, GameProductCategory category)
    : GameEntity(UexApiGameEntityId.Create<GameItem>(id), GameEntityCategory.Item), IGameManufactured, IGameTradable
{
    public const string CategoryPropertyName = "Category";

    public UexId<GameItem> StrongId
        => (Id as UexId<GameItem>)!;

    public TraitCollection Traits { get; } = new();

    public GameCompany Manufacturer
        => manufacturer;

    public override GameEntityName Name
        => new(
            GameEntityName.ReferenceTo(category),
            GameEntityName.ReferenceTo(manufacturer),
            GameEntityName.PropertyCollection.Create([new GameEntityName.PropertyItem(CategoryPropertyName, category.Name.MainContent.FullName)], Traits, []),
            new GameEntityName.Name(fullName)
        );

    public GameTerminalType TerminalType
        => GameTerminalType.Item;

    public Bounds<PriceTag> LatestPurchasePrices { get; private set; } = new(PriceTag.Unknown, PriceTag.Unknown, PriceTag.Unknown);

    public Bounds<PriceTag> LatestSalePrices { get; private set; } = new(PriceTag.Unknown, PriceTag.Unknown, PriceTag.Unknown);

    public void UpdatePurchasePrices(Bounds<PriceTag> newPrices)
        => LatestPurchasePrices = newPrices;

    public void UpdateSalePrices(Bounds<PriceTag> newPrices)
        => LatestSalePrices = newPrices;

    protected override IEnumerable<SearchableTrait> CollectSearchableTraits()
    {
        yield return new SearchableName(fullName);
        yield return new SearchableName(category.Name.MainContent.FullName);
        yield return new SearchableManufacturer(manufacturer);
        foreach (var searchableAttribute in base.CollectSearchableTraits())
        {
            yield return searchableAttribute;
        }
    }

    public class TraitCollection : IEnumerable<GameItemTrait>
    {
        private List<GameItemTrait> _traits = [];

        public IEnumerator<GameItemTrait> GetEnumerator()
            => _traits.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void Update(IEnumerable<GameItemTrait> traits)
            => _traits = traits.ToList();
    }
}
