namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Abstractions.Game;
using Attributes;
using Enums;
using Search;
using Trade;

[Description("Game Commodity Entry")]
[CacheEntryDescription("Game Commodities")]
public class GameCommodity(int id, string fullName, string codeName)
    : GameEntity(UexApiGameEntityId.Create<GameCommodity>(id), GameEntityCategory.Commodity), IGameTradable
{
    public UexId<GameCommodity> StrongId
        => (Id as UexId<GameCommodity>)!;

    public override GameEntityName Name { get; } = new(new GameEntityName.NameWithCode(fullName, codeName));

    public Bounds<PriceTag> LatestPurchasePrices { get; private set; } = new(PriceTag.Unknown, PriceTag.Unknown, PriceTag.Unknown);

    public Bounds<PriceTag> LatestSalePrices { get; private set; } = new(PriceTag.Unknown, PriceTag.Unknown, PriceTag.Unknown);

    public GameTerminalType TerminalType
        => GameTerminalType.Commodity;

    public void UpdateSalePrices(Bounds<PriceTag> newPrices)
        => LatestSalePrices = newPrices;

    public void UpdatePurchasePrices(Bounds<PriceTag> newPrices)
        => LatestPurchasePrices = newPrices;

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
