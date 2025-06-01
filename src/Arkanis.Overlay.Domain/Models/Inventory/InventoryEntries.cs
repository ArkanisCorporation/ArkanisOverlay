namespace Arkanis.Overlay.Domain.Models.Inventory;

using Abstractions;
using Abstractions.Game;
using Game;

public record InventoryEntryId(Guid Identity) : TypedDomainId<Guid>(Identity)
{
    public static InventoryEntryId CreateNew()
        => new(Guid.NewGuid());
}

public static class InventoryEntry
{
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
