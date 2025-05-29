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
    public static VirtualItemInventoryEntry Create(GameItem gameItem, Quantity quantity)
        => new()
        {
            Item = gameItem,
            Quantity = quantity,
        };

    public static PhysicalItemInventoryEntry CreateAt(GameItem gameItem, Quantity quantity, IGameLocation location)
        => new()
        {
            Item = gameItem,
            Quantity = quantity,
            Location = location,
        };

    public static VirtualCommodityInventoryEntry Create(GameCommodity gameCommodity, Quantity quantity)
        => new()
        {
            Commodity = gameCommodity,
            Quantity = quantity,
        };

    public static PhysicalCommodityInventoryEntry CreateAt(GameCommodity gameCommodity, Quantity quantity, IGameLocation location)
        => new()
        {
            Commodity = gameCommodity,
            Quantity = quantity,
            Location = location,
        };
}

public abstract class InventoryEntryBase : IIdentifiable
{
    public InventoryEntryId Id { get; init; } = InventoryEntryId.CreateNew();

    public abstract IGameEntity Entity { get; }

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
    public override InventoryEntryBase SetLocation(IGameLocation location)
        => new PhysicalItemInventoryEntry
        {
            Item = Item,
            Quantity = Quantity,
            Location = location,
        };
}

public sealed class PhysicalItemInventoryEntry : ItemInventoryEntry, IGameLocatedAt
{
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
    public override InventoryEntryBase SetLocation(IGameLocation location)
        => new PhysicalCommodityInventoryEntry
        {
            Commodity = Commodity,
            Quantity = Quantity,
            Location = location,
        };
}

public sealed class PhysicalCommodityInventoryEntry : CommodityInventoryEntry, IGameLocatedAt
{
    public required IGameLocation Location { get; set; }

    public override InventoryEntryBase SetLocation(IGameLocation location)
    {
        Location = location;
        return this;
    }
}
