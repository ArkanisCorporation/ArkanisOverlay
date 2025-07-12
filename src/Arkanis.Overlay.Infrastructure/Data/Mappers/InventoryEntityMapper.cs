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
            VirtualInventoryEntry entry => Map(entry, referenceHandler),
            LocationInventoryEntry entry => Map(entry, referenceHandler),
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

    #endregion
}
