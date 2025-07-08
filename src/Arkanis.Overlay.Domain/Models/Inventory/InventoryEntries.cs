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
    public static VirtualInventoryEntry Create(QuantityOf quantityOf, InventoryEntryList? list = null)
        => new()
        {
            Quantity = quantityOf,
            List = list,
        };

    public static LocationInventoryEntry Create(QuantityOf quantityOf, IGameLocation location, InventoryEntryList? list = null)
        => new()
        {
            Quantity = quantityOf,
            Location = location,
            List = list,
        };

    public static VirtualInventoryEntry Create(GameItem item, Quantity quantity, InventoryEntryList? list = null)
        => Create(QuantityOf.Create(item, quantity), list);

    public static VirtualInventoryEntry Create(GameCommodity item, Quantity quantity, InventoryEntryList? list = null)
        => Create(QuantityOf.Create(item, quantity), list);

    public static LocationInventoryEntry CreateAt(GameItem item, Quantity quantity, IGameLocation location, InventoryEntryList? list = null)
        => Create(QuantityOf.Create(item, quantity), location, list);

    public static LocationInventoryEntry CreateAt(GameCommodity item, Quantity quantity, IGameLocation location, InventoryEntryList? list = null)
        => Create(QuantityOf.Create(item, quantity), location, list);

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

        return Create(source.Quantity.Reference.Entity, quantity, location, list);
    }
}

public abstract class InventoryEntryBase : IIdentifiable
{
    public enum EntryType
    {
        Undefined,
        Virtual,
        Location,
        Hangar,
        VehicleModule,
        VehicleInventory,
    }

    public InventoryEntryId Id { get; init; } = InventoryEntryId.CreateNew();

    public bool IsManagedInternally
        => TradeRun is not null;

    public IGameEntity Entity
        => Quantity.Reference.Entity;

    public TradeRun? TradeRun { get; set; }
    public InventoryEntryList? List { get; set; }

    public abstract EntryType Type { get; }

    public required QuantityOf Quantity { get; set; }

    IDomainId IIdentifiable.Id
        => Id;

    public abstract InventoryEntryBase SetLocation(IGameLocation location);
}

public sealed class VirtualInventoryEntry : InventoryEntryBase
{
    public override EntryType Type
        => EntryType.Virtual;

    public override InventoryEntryBase SetLocation(IGameLocation location)
        => new LocationInventoryEntry
        {
            Id = Id,
            Quantity = Quantity,
            Location = location,
            List = List,
        };
}

public sealed class LocationInventoryEntry : InventoryEntryBase, IGameLocatedAt
{
    public override EntryType Type
        => EntryType.Location;

    public required IGameLocation Location { get; set; }

    public override InventoryEntryBase SetLocation(IGameLocation location)
    {
        Location = location;
        return this;
    }
}

public sealed class HangarInventoryEntry : InventoryEntryBase, IGameLocatedAt
{
    public override EntryType Type
        => EntryType.Hangar;

    public GameVehicle VehicleReference
        => Quantity.ReferenceAs<GameVehicle>();

    public UexHangarEntryId? UexHangarEntryId { get; set; }

    public string? NameTag { get; set; }
    public bool IsPledged { get; set; }
    public bool IsLoaner { get; set; }

    public List<VehicleModuleEntry> Modules { get; set; } = [];
    public List<VehicleInventoryEntry> Inventory { get; set; } = [];

    public required IGameLocation Location { get; set; }

    public override InventoryEntryBase SetLocation(IGameLocation location)
    {
        Location = location;
        return this;
    }
}

public sealed class VehicleModuleEntry : InventoryEntryBase
{
    public override EntryType Type
        => EntryType.VehicleModule;

    public string CategoryName
        => Quantity.ReferenceAs<GameItem>().Name.OfType<GameEntityName.ItemCategoryReference>().FirstOrDefault()?.Category.Name.MainContent.FullName
           ?? "Unknown";

    public override InventoryEntryBase SetLocation(IGameLocation location)
        => new LocationInventoryEntry
        {
            Id = Id,
            Quantity = Quantity,
            Location = location,
            List = List,
        };
}

public sealed class VehicleInventoryEntry : InventoryEntryBase
{
    public override EntryType Type
        => EntryType.VehicleInventory;

    public override InventoryEntryBase SetLocation(IGameLocation location)
        => new LocationInventoryEntry
        {
            Id = Id,
            Quantity = Quantity,
            Location = location,
            List = List,
        };
}
