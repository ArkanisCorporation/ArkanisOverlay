namespace Arkanis.Overlay.Infrastructure.Data.Mappers;

using Domain.Models.Inventory;
using Entities;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, UseReferenceHandling = true)]
internal partial class InventoryEntityMapper(
    EntityReferenceMapper entityReferenceMapper,
    TradeRunEntityMapper tradeRunEntityMapper,
    UexApiDtoMapper uexMapper
) : TradeRunEntityMapper.ICapabilities,
    UexApiDtoMapper.ICapabilities,
    EntityReferenceMapper.ICapabilities
{
    UexApiDtoMapper IMapperWith<UexApiDtoMapper>.Reference
        => uexMapper;

    TradeRunEntityMapper IMapperWith<TradeRunEntityMapper>.Reference
        => tradeRunEntityMapper;

    EntityReferenceMapper IMapperWith<EntityReferenceMapper>.Reference
        => entityReferenceMapper;

    #region (To Database) Inventory List Mapping

    public partial InventoryEntryListEntity Map(InventoryEntryList list);

    #endregion

    #region (To Domain) Inventory List Mapping

    public partial InventoryEntryList Map(InventoryEntryListEntity list);

    public partial InventoryEntryListBrief MapBrief(InventoryEntryListEntity list);

    #endregion

    #region (To Database) Inventory Entry Mapping

    [UserMapping(Default = true)]
    public InventoryEntryEntityBase Map(InventoryEntryBase entryBase, [ReferenceHandler] IReferenceHandler? referenceHandler = null)
    {
        referenceHandler ??= new PreserveReferenceHandler();
        return entryBase switch
        {
            VirtualInventoryEntry entry => Map(entry, referenceHandler),
            LocationInventoryEntry entry => Map(entry, referenceHandler),
            HangarInventoryEntry entry => Map(entry, referenceHandler),
            VehicleModuleEntry entry => Map(entry, referenceHandler),
            VehicleInventoryEntry entry => Map(entry, referenceHandler),
            // others
            _ => throw new NotSupportedException($"Database entity mapping not supported for source domain object: {entryBase}"),
        };
    }

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.EntryType))]
    [MapValue(nameof(InventoryEntryEntityBase.List), null)]
    private partial VirtualInventoryEntryEntity Map(
        VirtualInventoryEntry entry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.EntryType))]
    [MapValue(nameof(InventoryEntryEntityBase.List), null)]
    private partial LocationInventoryEntryEntity Map(
        LocationInventoryEntry entry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.EntryType))]
    [MapValue(nameof(InventoryEntryEntityBase.List), null)]
    private partial HangarInventoryEntryEntity Map(
        HangarInventoryEntry entry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.EntryType))]
    [MapValue(nameof(InventoryEntryEntityBase.List), null)]
    [MapValue(nameof(VehicleModuleEntryEntity.HangarEntry), null)]
    private partial VehicleModuleEntryEntity Map(
        VehicleModuleEntry entry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.EntryType))]
    [MapValue(nameof(InventoryEntryEntityBase.List), null)]
    [MapValue(nameof(VehicleInventoryEntryEntity.HangarEntry), null)]
    private partial VehicleInventoryEntryEntity Map(
        VehicleInventoryEntry entry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    #endregion

    #region (To Domain) Inventory Entry Entity Mappings

    [UserMapping(Default = true)]
    public InventoryEntryBase Map(InventoryEntryEntityBase entryBase, [ReferenceHandler] IReferenceHandler? referenceHandler = null)
    {
        referenceHandler ??= new PreserveReferenceHandler();
        return entryBase switch
        {
            VirtualInventoryEntryEntity entry => Map(entry, referenceHandler),
            LocationInventoryEntryEntity entry => Map(entry, referenceHandler),
            HangarInventoryEntryEntity entry => Map(entry, referenceHandler),
            VehicleModuleEntryEntity entry => Map(entry, referenceHandler),
            VehicleInventoryEntryEntity entry => Map(entry, referenceHandler),
            // others
            _ => throw new NotSupportedException($"Domain entity mapping not supported for source database object: {entryBase}"),
        };
    }

    private partial VirtualInventoryEntry Map(
        VirtualInventoryEntryEntity bareEntry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapProperty(nameof(LocationInventoryEntryEntity.LocationId), nameof(LocationInventoryEntry.Location))]
    private partial LocationInventoryEntry Map(
        LocationInventoryEntryEntity bareEntry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapProperty(nameof(HangarInventoryEntryEntity.LocationId), nameof(HangarInventoryEntry.Location))]
    [MapperIgnoreTarget(nameof(HangarInventoryEntry.IsLoaner))]
    private partial HangarInventoryEntry Map(
        HangarInventoryEntryEntity bareEntry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    //? Maps a hangar entry WITHOUT its Modules/Inventory back-references.
    //  Used exclusively for the required HangarEntry navigation of vehicle-placed entries below.
    //  Mapperly assigns required members inside the target's object initializer, i.e. BEFORE the
    //  vehicle entry registers its own reference. If the mapped hangar then walked its Modules/Inventory
    //  collections (which contain that very vehicle entry) the reference handler could not yet
    //  de-duplicate it, producing unbounded recursion. Omitting those collections here breaks the cycle;
    //  when a hangar is instead mapped as a top-level entry its full mapping above is used, and any
    //  nested vehicle entry resolves back to it through the reference handler.
    [MapProperty(nameof(HangarInventoryEntryEntity.LocationId), nameof(HangarInventoryEntry.Location))]
    [MapperIgnoreTarget(nameof(HangarInventoryEntry.IsLoaner))]
    [MapperIgnoreTarget(nameof(HangarInventoryEntry.Modules))]
    [MapperIgnoreTarget(nameof(HangarInventoryEntry.Inventory))]
    private partial HangarInventoryEntry MapAsReference(
        HangarInventoryEntryEntity bareEntry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapProperty(nameof(VehicleModuleEntryEntity.HangarEntry), nameof(VehicleModuleEntry.HangarEntry), Use = nameof(MapAsReference))]
    private partial VehicleModuleEntry Map(
        VehicleModuleEntryEntity bareEntry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapProperty(nameof(VehicleInventoryEntryEntity.HangarEntry), nameof(VehicleInventoryEntry.HangarEntry), Use = nameof(MapAsReference))]
    private partial VehicleInventoryEntry Map(
        VehicleInventoryEntryEntity bareEntry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    #endregion
}
