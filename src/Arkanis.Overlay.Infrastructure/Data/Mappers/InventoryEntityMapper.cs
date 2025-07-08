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

    #endregion

    #region (To Database) Inventory Entry Mapping

    [UserMapping(Default = true)]
    public InventoryEntryEntityBase Map(InventoryEntryBase entryBase, [ReferenceHandler] IReferenceHandler? referenceHandler = null)
    {
        referenceHandler ??= new PreserveReferenceHandler();
        return entryBase switch
        {
            // items
            VirtualItemInventoryEntry entry => Map(entry, referenceHandler),
            PhysicalItemInventoryEntry entry => Map(entry, referenceHandler),
            // commodities
            VirtualCommodityInventoryEntry entry => Map(entry, referenceHandler),
            PhysicalCommodityInventoryEntry entry => Map(entry, referenceHandler),
            // others
            _ => throw new NotSupportedException($"Database entity mapping not supported for source domain object: {entryBase}"),
        };
    }

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityId))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityCategory))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.EntryType))]
    [MapValue(nameof(InventoryEntryEntityBase.List), null)]
    private partial VirtualItemInventoryEntryEntity Map(
        VirtualItemInventoryEntry entry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityId))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityCategory))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.EntryType))]
    [MapValue(nameof(InventoryEntryEntityBase.List), null)]
    private partial PhysicalItemInventoryEntryEntity Map(
        PhysicalItemInventoryEntry entry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityId))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityCategory))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.EntryType))]
    [MapValue(nameof(InventoryEntryEntityBase.List), null)]
    private partial VirtualCommodityInventoryEntryEntity Map(
        VirtualCommodityInventoryEntry entry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityId))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityCategory))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.EntryType))]
    [MapValue(nameof(InventoryEntryEntityBase.List), null)]
    private partial PhysicalCommodityInventoryEntryEntity Map(
        PhysicalCommodityInventoryEntry entry,
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
            // items
            VirtualItemInventoryEntryEntity entry => Map(entry, referenceHandler),
            PhysicalItemInventoryEntryEntity entry => Map(entry, referenceHandler),
            // commodities
            VirtualCommodityInventoryEntryEntity entry => Map(entry, referenceHandler),
            PhysicalCommodityInventoryEntryEntity entry => Map(entry, referenceHandler),
            // others
            _ => throw new NotSupportedException($"Domain entity mapping not supported for source database object: {entryBase}"),
        };
    }

    [MapProperty(nameof(ItemInventoryEntryEntityBase.ItemId), nameof(VirtualItemInventoryEntry.Item))]
    private partial VirtualItemInventoryEntry Map(
        VirtualItemInventoryEntryEntity bareEntry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapProperty(nameof(ItemInventoryEntryEntityBase.ItemId), nameof(PhysicalItemInventoryEntry.Item))]
    [MapProperty(nameof(PhysicalItemInventoryEntryEntity.LocationId), nameof(PhysicalItemInventoryEntry.Location))]
    private partial PhysicalItemInventoryEntry Map(
        PhysicalItemInventoryEntryEntity bareEntry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapProperty(nameof(CommodityInventoryEntryEntityBase.CommodityId), nameof(VirtualCommodityInventoryEntry.Commodity))]
    private partial VirtualCommodityInventoryEntry Map(
        VirtualCommodityInventoryEntryEntity bareEntry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    [MapProperty(nameof(CommodityInventoryEntryEntityBase.CommodityId), nameof(PhysicalCommodityInventoryEntry.Commodity))]
    [MapProperty(nameof(PhysicalCommodityInventoryEntryEntity.LocationId), nameof(PhysicalCommodityInventoryEntry.Location))]
    private partial PhysicalCommodityInventoryEntry Map(
        PhysicalCommodityInventoryEntryEntity bareEntry,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );

    #endregion
}
