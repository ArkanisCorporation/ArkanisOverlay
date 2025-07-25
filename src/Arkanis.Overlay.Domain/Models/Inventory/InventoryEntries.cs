namespace Arkanis.Overlay.Domain.Models.Inventory;

using Abstractions;
using Abstractions.Game;
using Game;
using Trade;

public record InventoryEntryId(Guid Identity) : TypedDomainId<Guid>(Identity)
{
    public static InventoryEntryId CreateNew()
        => new(Guid.NewGuid());
}

public static class InventoryEntry
{
    public static InventoryEntryBase Create(QuantityOf quantityOf, InventoryEntryList? list = null)
        => quantityOf.Reference switch
        {
            OwnableEntityReference.Commodity reference => Create(reference.Entity, quantityOf, list),
            OwnableEntityReference.Item reference => Create(reference.Entity, quantityOf, list),
            _ => throw new NotSupportedException($"Unable to create appropriate inventory entry for: {quantityOf.Reference}"),
        };

    public static VirtualItemInventoryEntry Create(GameItem item, Quantity quantity, InventoryEntryList? list = null)
        => new()
        {
            Item = item,
            Quantity = quantity,
            List = list,
        };

    public static PhysicalItemInventoryEntry CreateAt(GameItem item, Quantity quantity, IGameLocation location, InventoryEntryList? list = null)
        => new()
        {
            Item = item,
            Quantity = quantity,
            Location = location,
            List = list,
        };

    public static VirtualCommodityInventoryEntry Create(GameCommodity commodity, Quantity quantity, InventoryEntryList? list = null)
        => new()
        {
            Commodity = commodity,
            Quantity = quantity,
            List = list,
        };

    public static PhysicalCommodityInventoryEntry CreateAt(GameCommodity commodity, Quantity quantity, IGameLocation location, InventoryEntryList? list = null)
        => new()
        {
            Commodity = commodity,
            Quantity = quantity,
            Location = location,
            List = list,
        };

    public static InventoryEntryBase Create(IGameEntity source, Quantity quantity, IGameLocation? location = null, InventoryEntryList? list = null)
        => source switch
        {
            GameItem item => location is not null
                ? CreateAt(item, quantity, location, list)
                : Create(item, quantity, list),
            GameCommodity commodity => location is not null
                ? CreateAt(commodity, quantity, location, list)
                : Create(commodity, quantity, list),
            _ => throw new NotSupportedException($"Unable to create appropriate inventory entry for: {source}"),
        };

    public static InventoryEntryBase CreateFrom(
        InventoryEntryBase source,
        Quantity? quantity = null,
        IGameLocation? location = null,
        InventoryEntryList? list = null
    )
    {
        quantity ??= source.Quantity;
        list ??= source.List;
        if (source is IGameLocatedAt locatedAt)
        {
            location ??= locatedAt.Location;
        }

        return Create(source.Entity, quantity, location, list);
    }
}

public abstract class InventoryEntryBase : IIdentifiable
{
    public enum EntryType
    {
        Undefined,
        Virtual,
        Physical,
    }

    public InventoryEntryId Id { get; init; } = InventoryEntryId.CreateNew();

    public InventoryEntryList? List { get; set; }

    public abstract IGameEntity Entity { get; }

    public abstract EntryType Type { get; }

    public required Quantity Quantity { get; set; }

    IDomainId IIdentifiable.Id
        => Id;

    public abstract InventoryEntryBase SetLocation(IGameLocation location);
}

public abstract class ItemInventoryEntry : InventoryEntryBase
{
    public required GameItem Item { get; init; }

    public override IGameEntity Entity
        => Item;
}

public sealed class VirtualItemInventoryEntry : ItemInventoryEntry
{
    public override EntryType Type
        => EntryType.Virtual;

    public override InventoryEntryBase SetLocation(IGameLocation location)
        => new PhysicalItemInventoryEntry
        {
            Id = Id,
            Item = Item,
            Quantity = Quantity,
            Location = location,
            List = List,
        };
}

public sealed class PhysicalItemInventoryEntry : ItemInventoryEntry, IGameLocatedAt
{
    public override EntryType Type
        => EntryType.Physical;

    public required IGameLocation Location { get; set; }

    public override InventoryEntryBase SetLocation(IGameLocation location)
    {
        Location = location;
        return this;
    }
}

public abstract class CommodityInventoryEntry : InventoryEntryBase
{
    public required GameCommodity Commodity { get; init; }

    public override IGameEntity Entity
        => Commodity;
}

public sealed class VirtualCommodityInventoryEntry : CommodityInventoryEntry
{
    public override EntryType Type
        => EntryType.Virtual;

    public override InventoryEntryBase SetLocation(IGameLocation location)
        => new PhysicalCommodityInventoryEntry
        {
            Id = Id,
            Commodity = Commodity,
            Quantity = Quantity,
            Location = location,
            List = List,
        };
}

public sealed class PhysicalCommodityInventoryEntry : CommodityInventoryEntry, IGameLocatedAt
{
    public override EntryType Type
        => EntryType.Physical;

    public required IGameLocation Location { get; set; }

    public override InventoryEntryBase SetLocation(IGameLocation location)
    {
        Location = location;
        return this;
    }
}
