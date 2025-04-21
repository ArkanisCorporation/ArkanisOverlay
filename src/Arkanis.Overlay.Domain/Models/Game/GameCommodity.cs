namespace Arkanis.Overlay.Domain.Models.Game;

using Abstractions.Game;
using Enums;
using Search;
using Trade;

public class GameCommodity(int id, string fullName, string codeName)
    : GameEntity(UexApiGameEntityId.Create<GameCommodity>(id), GameEntityCategory.Commodity), IGameTradable
{
    public override IEnumerable<SearchableTrait> SearchableAttributes
    {
        get
        {
            yield return new SearchableName(fullName);
            yield return new SearchableCode(codeName);
            foreach (var searchableAttribute in base.SearchableAttributes)
            {
                yield return searchableAttribute;
            }
        }
    }

    public override GameEntityName Name { get; } = new(new GameEntityName.NameWithCode(fullName, codeName));

    public Bounds<PriceTag> LatestPurchasePrices { get; private set; } = new(PriceTag.Unknown, PriceTag.Unknown, PriceTag.Unknown);

    public Bounds<PriceTag> LatestSellPrices { get; private set; } = new(PriceTag.Unknown, PriceTag.Unknown, PriceTag.Unknown);

    public GameTerminalType TerminalType
        => GameTerminalType.Commodity;

    public void UpdateSellPrices(Bounds<PriceTag> newPrices)
        => LatestSellPrices = newPrices;

    public void UpdatePurchasePrices(Bounds<PriceTag> newPrices)
        => LatestPurchasePrices = newPrices;
}
