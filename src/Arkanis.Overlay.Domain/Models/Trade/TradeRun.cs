namespace Arkanis.Overlay.Domain.Models.Trade;

using Enums;
using Game;
using Inventory;
using MoreLinq;

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
    public DateTimeOffset? FinishedAt { get; set; }

    public IEnumerable<QuantityOf> AcquiredQuantities
        => Acquisitions.GroupBy(x => x.Quantity.Reference.EntityId)
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

    public abstract class Stage
    {
        public GameCurrency PricePerUnit { get; set; } = GameCurrency.Zero;

        public GameCurrency PriceTotal
            => PricePerUnit * Quantity.Amount;

        public abstract GameCurrency Balance { get; }

        public required QuantityOf Quantity { get; set; }
        public bool UsedAutoload { get; set; }

        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? ReachedAt { get; set; }
        public DateTimeOffset? TransferredAt { get; set; }
        public DateTimeOffset? FinalizedAt { get; set; }
    }

    public abstract class AcquisitionStage : Stage
    {
        public DateTimeOffset? AcquiredAt { get; set; }

        public override GameCurrency Balance
            => PricePerUnit * Quantity.Amount * -1;
    }

    public sealed class TerminalPurchaseStage : AcquisitionStage
    {
        public required GameTerminal Terminal { get; set; }

        // TODO: relevant terminal data

        public bool SourcedDataConfirmed { get; set; }
        public GameContainerSize MaxContainerSize { get; set; }
        public TerminalInventoryStatus StockStatus { get; set; }
        public Quantity Stock { get; set; } = Inventory.Quantity.FromScu(0);
    }

    public abstract class SaleStage : Stage
    {
        public DateTimeOffset? SoldAt { get; set; }

        public override GameCurrency Balance
            => PricePerUnit * Quantity.Amount;
    }

    public sealed class TerminalSaleStage : SaleStage
    {
        public required GameTerminal Terminal { get; set; }

        // TODO: relevant terminal data

        public bool SourcedDataConfirmed { get; set; }
        public GameContainerSize MaxContainerSize { get; set; }
        public TerminalInventoryStatus StockStatus { get; set; }
        public Quantity Stock { get; set; } = Inventory.Quantity.FromScu(0);
    }
}

public record QuantityOf(OwnableEntityReference Reference, int Amount, Quantity.UnitType Unit) : Quantity(Amount, Unit)
{
    public OwnableEntityReference Reference { get; set; } = Reference;

    public override string ToString()
        => base.ToString();
}
