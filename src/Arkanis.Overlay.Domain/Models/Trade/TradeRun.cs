namespace Arkanis.Overlay.Domain.Models.Trade;

using Enums;
using Game;
using Inventory;
using MoreLinq;
using MudBlazor.FontIcons.MaterialIcons;

public record TradeRunId(Guid Identity) : TypedDomainId<Guid>(Identity)
{
    public static TradeRunId CreateNew()
        => new(Guid.NewGuid());
}

public class TradeRun
{
    public TradeRunId TradeRunId { get; init; } = TradeRunId.CreateNew();

    public AcquisitionStage[] Acquisitions { get; set; } = [];
    public List<SaleStage> Sales { get; set; } = [];

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? FinalizedAt { get; set; }

    public Stage? StageInProgress
        => Stages.FirstOrDefault(x => x.FinalizedAt is null);

    public IEnumerable<QuantityOf> AcquiredQuantities
        => Acquisitions
            .Where(x => x.FinalizedAt is not null)
            .GroupBy(x => x.Quantity.Reference.EntityId)
            .Select(amounts => new QuantityOf(
                    amounts.First().Quantity.Reference,
                    amounts.Select(amount => amount.Quantity.Amount).Sum(),
                    amounts.First().Quantity.Unit
                )
            );

    public IEnumerable<QuantityOf> SoldQuantities
        => Sales
            .Where(x => x.FinalizedAt is not null)
            .GroupBy(x => x.Quantity.Reference.EntityId)
            .Select(amounts => new QuantityOf(
                    amounts.First().Quantity.Reference,
                    amounts.Select(amount => amount.Quantity.Amount).Sum(),
                    amounts.First().Quantity.Unit
                )
            );

    public IEnumerable<QuantityOf> UnsoldQuantities
        => AcquiredQuantities.LeftJoin(
            SoldQuantities,
            acquired => acquired.Reference.EntityId,
            acquired => acquired,
            (acquired, sold) => new QuantityOf(acquired.Reference, acquired.Amount - sold.Amount, acquired.Unit)
        );

    public GameCurrency Investment
        => new(-1 * Acquisitions.Sum(x => x.PriceTotal.Amount));

    public GameCurrency Revenue
        => new(Sales.Sum(x => x.PriceTotal.Amount));

    public GameCurrency Profit
        => Revenue + Investment;

    public IEnumerable<Stage> Stages
        => Acquisitions
            .OfType<Stage>()
            .Concat(Sales);

    public bool HasUnsoldCargo
        => UnsoldQuantities.Any(x => x.Amount > 0);

    public Quantity AcquiredQuantityOf(UexApiGameEntityId gameEntityId)
        => AcquiredQuantities
            .Where(x => x.Reference.EntityId == gameEntityId)
            .SingleOrDefault(Quantity.Zero);

    public static TradeRun Create(GameTradeRoute tradeRoute)
        => new()
        {
            Acquisitions = [TerminalPurchaseStage.Create(tradeRoute)],
            Sales = [TerminalSaleStage.Create(tradeRoute)],
        };

    public IEnumerable<PlayerEvent> CreateEvents()
    {
        yield return new PlayerEvent.RunStarted(CreatedAt);

        foreach (var playerEvent in Stages.SelectMany(x => x.CreateEvents()).OrderBy(x => x.OccuredAt))
        {
            yield return playerEvent;
        }

        if (FinalizedAt is not null)
        {
            yield return new PlayerEvent.RunCompleted(FinalizedAt.Value);
        }
    }

    public abstract class PlayerEvent(DateTimeOffset occuredAt)
    {
        public abstract string Text { get; }
        public abstract string Icon { get; }

        public DateTimeOffset OccuredAt { get; } = occuredAt;

        public class RunStarted(DateTimeOffset occuredAt) : PlayerEvent(occuredAt)
        {
            public override string Text
                => "Trade run started";

            public override string Icon
                => Filled.Check;
        }

        public class RunCompleted(DateTimeOffset occuredAt) : PlayerEvent(occuredAt)
        {
            public override string Text
                => "Trade run finalized";

            public override string Icon
                => Filled.Start;
        }

        public class StageCompleted(DateTimeOffset occuredAt) : PlayerEvent(occuredAt)
        {
            public override string Text
                => "Trade part completed";

            public override string Icon
                => Filled.Check;
        }

        public class Takeoff(DateTimeOffset occuredAt) : PlayerEvent(occuredAt)
        {
            public override string Text
                => "Started travelling";

            public override string Icon
                => Filled.FlightTakeoff;
        }

        public class Landing(DateTimeOffset occuredAt) : PlayerEvent(occuredAt)
        {
            public override string Text
                => "Reached destination";

            public override string Icon
                => Filled.FlightLand;
        }

        public class CargoAcquired(QuantityOf quantity, DateTimeOffset occuredAt) : PlayerEvent(occuredAt)
        {
            public override string Text
                => $"Purchased {quantity}";

            public override string Icon
                => Filled.AddShoppingCart;
        }

        public class CargoSold(QuantityOf quantity, DateTimeOffset occuredAt) : PlayerEvent(occuredAt)
        {
            public override string Text
                => $"Sold {quantity}";

            public override string Icon
                => Filled.AttachMoney;
        }

        public class CargoTransferred(QuantityOf quantity, DateTimeOffset occuredAt) : PlayerEvent(occuredAt)
        {
            public override string Text
                => $"Finished transferring {quantity}";

            public override string Icon
                => Filled.Forklift;
        }
    }

    public abstract class Stage
    {
        public GameCurrency PricePerUnit { get; set; } = GameCurrency.Zero;

        public GameCurrency PriceTotal
            => PricePerUnit * Quantity.Amount;

        public abstract GameCurrency Balance { get; }
        public abstract string Title { get; }
        public abstract string CurrentStepTitle { get; }

        public required QuantityOf Quantity { get; set; }
        public bool UsedAutoload { get; set; }

        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? ReachedAt { get; set; }
        public DateTimeOffset? TransferredAt { get; set; }
        public DateTimeOffset? FinalizedAt { get; set; }

        public abstract IEnumerable<PlayerEvent> CreateEvents();
    }

    public abstract class AcquisitionStage : Stage
    {
        public DateTimeOffset? AcquiredAt { get; set; }

        public override GameCurrency Balance
            => PricePerUnit * Quantity.Amount * -1;

        public override string CurrentStepTitle
            => this switch
            {
                { ReachedAt: null } => "Travel to the destination",
                { AcquiredAt: null } => "Acquire the cargo",
                { TransferredAt: null } => "Load the acquired cargo",
                { FinalizedAt: null } => "Prepare for takeoff",
                { FinalizedAt: not null } => "This stage has been finished",
                _ => "Continue",
            };

        public override IEnumerable<PlayerEvent> CreateEvents()
        {
            if (StartedAt is not null)
            {
                yield return new PlayerEvent.Takeoff(StartedAt.Value);
            }

            if (ReachedAt is not null)
            {
                yield return new PlayerEvent.Landing(ReachedAt.Value);
            }

            if (AcquiredAt is not null)
            {
                yield return new PlayerEvent.CargoAcquired(Quantity, AcquiredAt.Value);
            }

            if (TransferredAt is not null)
            {
                yield return new PlayerEvent.CargoTransferred(Quantity, TransferredAt.Value);
            }

            if (FinalizedAt is not null)
            {
                yield return new PlayerEvent.StageCompleted(FinalizedAt.Value);
            }
        }
    }

    public sealed class TerminalPurchaseStage : AcquisitionStage
    {
        public override string Title
            => $"Purchase {Quantity.ToString(QuantityOf.FormatWithReferenceCode, null)} at {Terminal.Name.MainContent.FullName}";

        public required GameTerminal Terminal { get; set; }

        // TODO: relevant terminal data

        public bool SourcedDataConfirmed { get; set; }
        public GameContainerSize MaxContainerSize { get; set; }
        public TerminalInventoryStatus StockStatus { get; set; }
        public Quantity Stock { get; set; } = Inventory.Quantity.FromScu(0);

        public static TerminalPurchaseStage Create(GameTradeRoute tradeRoute)
        {
            var commodity = new CommodityReference(tradeRoute.Commodity);
            var currentStock = Inventory.Quantity.FromScu(tradeRoute.Origin.CargoUnitsAvailable);
            var commodityQuantity = new QuantityOf(commodity, currentStock);

            return new TerminalPurchaseStage
            {
                PricePerUnit = tradeRoute.Origin.Price,
                MaxContainerSize = tradeRoute.Origin.MaxContainerSize,
                StockStatus = tradeRoute.Origin.InventoryStatus,
                Stock = currentStock,
                Terminal = tradeRoute.Origin.Terminal,
                Quantity = commodityQuantity,
            };
        }
    }

    public abstract class SaleStage : Stage
    {
        public DateTimeOffset? SoldAt { get; set; }

        public override GameCurrency Balance
            => PricePerUnit * Quantity.Amount;

        public override string CurrentStepTitle
            => this switch
            {
                { ReachedAt: null } => "Travel to the destination",
                { TransferredAt: null } when !UsedAutoload => "Unload the acquired cargo",
                { SoldAt: null } => "Sell the cargo",
                { TransferredAt: null } => "Wait for the cargo to unload",
                { FinalizedAt: null } => "Prepare for takeoff",
                { FinalizedAt: not null } => "This stage has been finished",
                _ => "Continue",
            };

        public override IEnumerable<PlayerEvent> CreateEvents()
        {
            if (StartedAt is not null)
            {
                yield return new PlayerEvent.Takeoff(StartedAt.Value);
            }

            if (ReachedAt is not null)
            {
                yield return new PlayerEvent.Landing(ReachedAt.Value);
            }

            if (SoldAt is not null)
            {
                yield return new PlayerEvent.CargoSold(Quantity, SoldAt.Value);
            }

            if (TransferredAt is not null)
            {
                yield return new PlayerEvent.CargoTransferred(Quantity, TransferredAt.Value);
            }

            if (FinalizedAt is not null)
            {
                yield return new PlayerEvent.StageCompleted(FinalizedAt.Value);
            }
        }
    }

    public sealed class TerminalSaleStage : SaleStage
    {
        public override string Title
            => $"Sell {Quantity.ToString(QuantityOf.FormatWithReferenceCode, null)} at {Terminal.Name.MainContent.FullName}";

        public required GameTerminal Terminal { get; set; }

        // TODO: relevant terminal data

        public bool SourcedDataConfirmed { get; set; }
        public GameContainerSize MaxContainerSize { get; set; }
        public TerminalInventoryStatus StockStatus { get; set; }
        public Quantity Stock { get; set; } = Inventory.Quantity.FromScu(0);

        public static TerminalSaleStage Create(GameTradeRoute tradeRoute)
        {
            var commodity = new CommodityReference(tradeRoute.Commodity);
            var commodityQuantity = new QuantityOf(commodity, tradeRoute.Origin.CargoUnitsAvailable, Inventory.Quantity.UnitType.StandardCargoUnit);

            return new TerminalSaleStage
            {
                PricePerUnit = tradeRoute.Destination.Price,
                MaxContainerSize = tradeRoute.Destination.MaxContainerSize,
                StockStatus = tradeRoute.Destination.InventoryStatus,
                Terminal = tradeRoute.Destination.Terminal,
                Quantity = commodityQuantity,
            };
        }
    }
}
