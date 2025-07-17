namespace Arkanis.Overlay.Domain.Models.Inventory;

using Abstractions;
using Abstractions.Game;
using Game;
using Riok.Mapperly.Abstractions;
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

    public static HangarInventoryEntry CreateInHangar(QuantityOf quantityOf, IGameLocation location, InventoryEntryList? list = null)
        => new()
        {
            Quantity = quantityOf,
            Location = location,
            List = list,
        };

    public static VehicleInventoryEntry CreateVehicleCargo(QuantityOf quantityOf, HangarInventoryEntry hangarEntry, InventoryEntryList? list = null)
        => new()
        {
            Quantity = quantityOf,
            HangarEntry = hangarEntry,
            List = list,
        };

    public static VehicleModuleEntry CreateVehicleModule(QuantityOf quantityOf, HangarInventoryEntry hangarEntry, InventoryEntryList? list = null)
        => new()
        {
            Quantity = quantityOf,
            HangarEntry = hangarEntry,
            List = list,
        };

    public static VirtualInventoryEntry Create(GameItem item, Quantity quantity, InventoryEntryList? list = null)
        => Create(QuantityOf.Create(item, quantity), list);

    public static VirtualInventoryEntry Create(GameCommodity item, Quantity quantity, InventoryEntryList? list = null)
        => Create(QuantityOf.Create(item, quantity), list);

    public static HangarInventoryEntry Create(GameVehicle vehicle, InventoryEntryList? list = null)
        => CreateAt(vehicle, GameLocationEntity.Unknown, list);

    public static LocationInventoryEntry CreateAt(GameItem item, Quantity quantity, IGameLocation location, InventoryEntryList? list = null)
        => Create(QuantityOf.Create(item, quantity), location, list);

    public static LocationInventoryEntry CreateAt(GameCommodity item, Quantity quantity, IGameLocation location, InventoryEntryList? list = null)
        => Create(QuantityOf.Create(item, quantity), location, list);

    public static HangarInventoryEntry CreateAt(GameVehicle vehicle, IGameLocation location, InventoryEntryList? list = null)
        => CreateInHangar(QuantityOf.Create(vehicle, Quantity.Default), location, list);

    public static VehicleInventoryEntry CreateCargo(GameItem item, Quantity quantity, HangarInventoryEntry hangarEntry, InventoryEntryList? list = null)
        => CreateVehicleCargo(QuantityOf.Create(item, quantity), hangarEntry, list);

    public static VehicleInventoryEntry CreateCargo(
        GameCommodity commodity,
        Quantity quantity,
        HangarInventoryEntry hangarEntry,
        InventoryEntryList? list = null
    )
        => CreateVehicleCargo(QuantityOf.Create(commodity, quantity), hangarEntry, list);

    public static VehicleModuleEntry CreateModule(GameItem item, Quantity quantity, HangarInventoryEntry hangarEntry, InventoryEntryList? list = null)
        => CreateVehicleModule(QuantityOf.Create(item, quantity), hangarEntry, list);

    public static InventoryEntryBase Create(IGameEntity source, Quantity quantity, Context? context = null)
        => source switch
        {
            GameItem item => context switch
            {
                { HangarEntry: not null } => CreateCargo(item, quantity, context.HangarEntry, context.List),
                { Location: not null } => CreateAt(item, quantity, context.Location, context.List),
                _ => Create(item, quantity, context?.List),
            },
            GameCommodity commodity => context switch
            {
                { HangarEntry: not null } => CreateCargo(commodity, quantity, context.HangarEntry, context.List),
                { Location: not null } => CreateAt(commodity, quantity, context.Location, context.List),
                _ => Create(commodity, quantity, context?.List),
            },
            GameVehicle vehicle => context switch
            {
                { Location: not null } => CreateAt(vehicle, context.Location ?? GameLocationEntity.Unknown, context.List),
                _ => Create(vehicle, context?.List),
            },
            _ => throw new NotSupportedException($"Unable to create appropriate inventory entry for: {source}"),
        };

    public static InventoryEntryBase CreateFrom(
        InventoryEntryBase source,
        Quantity? quantity = null,
        IGameLocation? location = null,
        HangarInventoryEntry? hangarEntry = null,
        InventoryEntryList? list = null
    )
    {
        quantity ??= source.Quantity;
        list ??= source.List;
        if (source is IGameLocatedAt locatedAt)
        {
            location ??= locatedAt.Location;
        }

        return Create(
            source.Quantity.Reference.Entity,
            quantity,
            new Context
            {
                Location = location,
                HangarEntry = hangarEntry,
                List = list,
            }
        );
    }

    public class Context
    {
        public IGameLocation? Location { get; set; }
        public HangarInventoryEntry? HangarEntry { get; set; }
        public InventoryEntryList? List { get; set; }
    }
}

public abstract class InventoryEntryBase : IIdentifiable, IInventoryPlacement
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

    public abstract InventoryEntryBase TransferTo(IGameLocation location);

    public virtual InventoryEntryBase TransferTo(HangarInventoryEntry hangarEntry, VehicleInventoryType inventoryType)
        => inventoryType switch
        {
            VehicleInventoryType.Cargo => new VehicleInventoryEntry
            {
                Id = Id,
                HangarEntry = hangarEntry,
                Quantity = Quantity,
                List = List,
            },
            VehicleInventoryType.Module => new VehicleModuleEntry
            {
                Id = Id,
                HangarEntry = hangarEntry,
                Quantity = Quantity,
                List = List,
            },
            _ => this,
        };
}

public sealed class VirtualInventoryEntry : InventoryEntryBase
{
    public override EntryType Type
        => EntryType.Virtual;

    public override InventoryEntryBase TransferTo(IGameLocation location)
        => new LocationInventoryEntry
        {
            Id = Id,
            Quantity = Quantity,
            Location = location,
            List = List,
        };
}

public sealed class LocationInventoryEntry : InventoryEntryBase, ILocationInventory
{
    public override EntryType Type
        => EntryType.Location;

    public required IGameLocation Location { get; set; }

    public override InventoryEntryBase TransferTo(IGameLocation location)
    {
        Location = location;
        return this;
    }
}

public sealed class HangarInventoryEntry : InventoryEntryBase, ILocationInventory
{
    public override EntryType Type
        => EntryType.Hangar;

    public GameVehicle VehicleReference
        => Quantity.ReferenceAs<GameVehicle>();

    public UexHangarEntryId? UexHangarEntryId { get; set; }

    public string? NameTag { get; set; }
    public bool IsPledged { get; set; }

    [MapperIgnore]
    public bool IsLoaner { get; set; }

    public List<VehicleModuleEntry> Modules { get; set; } = [];
    public List<VehicleInventoryEntry> Inventory { get; set; } = [];

    public required IGameLocation Location { get; set; }

    public override InventoryEntryBase TransferTo(IGameLocation location)
    {
        Location = location;
        return this;
    }

    public override InventoryEntryBase TransferTo(HangarInventoryEntry hangarEntry, VehicleInventoryType inventoryType)
        => this;
}

public sealed class VehicleModuleEntry : InventoryEntryBase, IVehicleInventory
{
    public override EntryType Type
        => EntryType.VehicleModule;

    public GameItem ItemReference
        => Quantity.ReferenceAs<GameItem>();

    public GameProductCategory Category
        => ItemReference.Category;

    public required HangarInventoryEntry HangarEntry { get; set; }

    public override InventoryEntryBase TransferTo(IGameLocation location)
        => new LocationInventoryEntry
        {
            Id = Id,
            Quantity = Quantity,
            Location = location,
            List = List,
        };

    public override InventoryEntryBase TransferTo(HangarInventoryEntry hangarEntry, VehicleInventoryType inventoryType)
    {
        if (inventoryType is not VehicleInventoryType.Module)
        {
            return base.TransferTo(hangarEntry, inventoryType);
        }

        HangarEntry = hangarEntry;
        return this;
    }
}

public sealed class VehicleInventoryEntry : InventoryEntryBase, IVehicleInventory
{
    public override EntryType Type
        => EntryType.VehicleInventory;

    public required HangarInventoryEntry HangarEntry { get; set; }

    public override InventoryEntryBase TransferTo(IGameLocation location)
        => new LocationInventoryEntry
        {
            Id = Id,
            Quantity = Quantity,
            Location = location,
            List = List,
        };

    public override InventoryEntryBase TransferTo(HangarInventoryEntry hangarEntry, VehicleInventoryType inventoryType)
    {
        if (inventoryType is not VehicleInventoryType.Cargo)
        {
            return base.TransferTo(hangarEntry, inventoryType);
        }

        HangarEntry = hangarEntry;
        return this;
    }
}

public interface IInventoryPlacement
{
    IDomainId? LocationId
        => null;
}

public interface ILocationInventory : IInventoryPlacement, IGameLocatedAt
{
    IDomainId IInventoryPlacement.LocationId
        => Location.Id;
}

public interface IVehicleInventory : IInventoryPlacement
{
    HangarInventoryEntry HangarEntry { get; }

    IDomainId IInventoryPlacement.LocationId
        => HangarEntry.Id;
}

public enum VehicleInventoryType
{
    Undefined,
    Cargo,
    Module,
}
