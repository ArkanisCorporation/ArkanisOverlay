namespace Arkanis.Overlay.Domain.Models.Game;

using Abstractions.Game;
using Enums;
using Search;
using Trade;

public class GameItem(int id, string fullName, GameCompany manufacturer, GameProductCategory category)
    : GameEntity(UexApiGameEntityId.Create<GameItem>(id), GameEntityCategory.Item), IGameManufactured, IGamePurchasable
{
    public GameCompany Manufacturer
        => manufacturer;

    public override IEnumerable<SearchableTrait> SearchableAttributes
    {
        get
        {
            yield return new SearchableName(fullName);
            yield return new SearchableManufacturer(manufacturer);
            foreach (var searchableAttribute in base.SearchableAttributes)
            {
                yield return searchableAttribute;
            }
        }
    }

    public override GameEntityName Name { get; } = new(
        GameEntityName.ReferenceTo(category),
        GameEntityName.ReferenceTo(manufacturer),
        new GameEntityName.Name(fullName)
    );

    public Bounds<PriceTag> LatestPurchasePrices { get; private set; } = new(PriceTag.Unknown, PriceTag.Unknown, PriceTag.Unknown);

    public GameTerminalType TerminalType
        => GameTerminalType.Item;

    public void UpdatePurchasePrices(Bounds<PriceTag> newPrices)
        => LatestPurchasePrices = newPrices;
}
