namespace Arkanis.Overlay.Domain.Models.Game;

using System.Collections;
using Abstractions.Game;
using Enums;
using Search;
using Trade;

public class GameItem(int id, string fullName, GameCompany manufacturer, GameProductCategory category)
    : GameEntity(UexApiGameEntityId.Create<GameItem>(id), GameEntityCategory.Item), IGameManufactured, IGameTradable
{
    public UexId<GameItem> StrongId
        => (Id as UexId<GameItem>)!;

    public TraitCollection Traits { get; } = new();

    public GameCompany Manufacturer
        => manufacturer;

    public override GameEntityName Name
        => new(
            GameEntityName.ReferenceTo(category),
            GameEntityName.ReferenceTo(manufacturer),
            GameEntityName.PropertyCollection.Create(Traits),
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
