namespace Arkanis.Overlay.Domain.Models.Inventory;

using Abstractions;
using Abstractions.Game;
using Game;

public record InventoryEntryId(Guid Identity) : TypedDomainId<Guid>(Identity)
{
    public static InventoryEntryId CreateNew()
        => new(Guid.NewGuid());
}

public abstract class InventoryEntryBase : IIdentifiable
{
    public InventoryEntryId Id { get; init; } = InventoryEntryId.CreateNew();

    public abstract IGameEntity Entity { get; }

    public required Quantity Quantity { get; set; }

    IDomainId IIdentifiable.Id
        => Id;
}

public abstract class ItemInventoryEntry : InventoryEntryBase
{
    public required GameItem Item { get; set; }

    public override IGameEntity Entity
        => Item;
}

public sealed class VirtualItemInventoryEntry : ItemInventoryEntry;

public sealed class PhysicalItemInventoryEntry : ItemInventoryEntry, IGameLocatedAt
{
    public required IGameLocation Location { get; set; }
}

public abstract class CommodityInventoryEntry : InventoryEntryBase
{
    public required GameCommodity Commodity { get; set; }

    public override IGameEntity Entity
        => Commodity;
}

public sealed class VirtualCommodityInventoryEntry : CommodityInventoryEntry;

public sealed class PhysicalCommodityInventoryEntry : CommodityInventoryEntry, IGameLocatedAt
{
    public required IGameLocation Location { get; set; }
}
