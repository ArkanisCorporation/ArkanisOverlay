namespace Arkanis.Overlay.Domain.Models.Game;

using Enums;
using Search;

public class GameTradeRoute(int id, GameCommodity commodity)
    : GameEntity(UexApiGameEntityId.Create<GameTradeRoute>(id), GameEntityCategory.TradeRoute)
{
    public double PriceMarginPercent { get; init; }
    public double PriceReturnOnInvestmentPercent { get; init; }
    public double? Distance { get; init; }

    public GameCommodity Commodity { get; } = commodity;

    public required Party Origin { get; init; }

    public required Party Destination { get; init; }

    public override GameEntityName Name
        => new(
            new GameEntityName.Name(
                $"{Commodity.Name.MainContent.FullName} from {Origin.Terminal.Parent!.Name.MainContent.FullName} to {Destination.Terminal.Parent!.Name.MainContent.FullName}"
            )
        );

    protected override IEnumerable<SearchableTrait> CollectSearchableTraits()
    {
        yield return new SearchableName(Commodity.Name);
        yield return new SearchableLocation(Origin.Terminal);
        yield return new SearchableLocation(Destination.Terminal);
        foreach (var searchableAttribute in base.CollectSearchableTraits())
        {
            yield return searchableAttribute;
        }
    }

    public class Party
    {
        public required GameTerminal Terminal { get; set; }
        public required GameCurrency Price { get; set; }
        public required TerminalInventoryStatus InventoryStatus { get; set; }
        public required GameContainerSize MaxContainerSize { get; set; }
        public required int CargoUnitsAvailable { get; set; }
    }
}
