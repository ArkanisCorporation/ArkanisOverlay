namespace Arkanis.Overlay.Infrastructure.Data.Mappers;

using Domain.Abstractions.Game;
using Domain.Models.Game;
using Domain.Models.Inventory;
using Entities;
using Riok.Mapperly.Abstractions;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal partial class InventoryEntityMapper(UexApiDtoMapper uexMapper)
{
    private GameItem ResolveItem(UexId<GameItem> itemId)
        => uexMapper.ResolveCachedGameEntity(itemId);

    private GameCommodity ResolveCommodity(UexId<GameCommodity> commodityId)
        => uexMapper.ResolveCachedGameEntity(commodityId);

    private IGameLocation ResolveLocation(UexApiGameEntityId locationId)
        => uexMapper.ResolveCachedGameEntity<IGameLocation>(locationId);

    #region (To Database) Inventory List Mapping

    public partial InventoryEntryListEntity Map(InventoryEntryList list);

    #endregion

    #region (To Domain) Inventory List Mapping

    public partial InventoryEntryList Map(InventoryEntryListEntity list);

    #endregion

    #region (To Database) Inventory Entry Mapping

    public InventoryEntryEntityBase Map(InventoryEntryBase entryBase)
        => entryBase switch
        {
            // items
            VirtualItemInventoryEntry entry => Map(entry),
            PhysicalItemInventoryEntry entry => Map(entry),
            // commodities
            VirtualCommodityInventoryEntry entry => Map(entry),
            PhysicalCommodityInventoryEntry entry => Map(entry),
            // others
            _ => throw new NotSupportedException($"Database entity mapping not supported for source domain object: {entryBase}"),
        };

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityId))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    private partial VirtualItemInventoryEntryEntity Map(VirtualItemInventoryEntry bareEntry);

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityId))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    private partial PhysicalItemInventoryEntryEntity Map(PhysicalItemInventoryEntry bareEntry);

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityId))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    private partial VirtualCommodityInventoryEntryEntity Map(VirtualCommodityInventoryEntry bareEntry);

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityId))]
    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.Discriminator))]
    private partial PhysicalCommodityInventoryEntryEntity Map(PhysicalCommodityInventoryEntry bareEntry);

    #endregion

    #region (To Domain) Inventory Entry Entity Mappings

    public InventoryEntryBase Map(InventoryEntryEntityBase entryBase)
        => entryBase switch
        {
            // items
            VirtualItemInventoryEntryEntity entry => Map(entry),
            PhysicalItemInventoryEntryEntity entry => Map(entry),
            // commodities
            VirtualCommodityInventoryEntryEntity entry => Map(entry),
            PhysicalCommodityInventoryEntryEntity entry => Map(entry),
            // others
            _ => throw new NotSupportedException($"Domain entity mapping not supported for source database object: {entryBase}"),
        };

    [MapProperty(nameof(ItemInventoryEntryEntityBase.ItemId), nameof(VirtualItemInventoryEntry.Item), Use = nameof(ResolveItem))]
    private partial VirtualItemInventoryEntry Map(VirtualItemInventoryEntryEntity bareEntry);

    [MapProperty(nameof(ItemInventoryEntryEntityBase.ItemId), nameof(PhysicalItemInventoryEntry.Item), Use = nameof(ResolveItem))]
    [MapProperty(nameof(PhysicalItemInventoryEntryEntity.LocationId), nameof(PhysicalItemInventoryEntry.Location), Use = nameof(ResolveLocation))]
    private partial PhysicalItemInventoryEntry Map(PhysicalItemInventoryEntryEntity bareEntry);

    [MapProperty(nameof(CommodityInventoryEntryEntityBase.CommodityId), nameof(VirtualCommodityInventoryEntry.Commodity), Use = nameof(ResolveCommodity))]
    private partial VirtualCommodityInventoryEntry Map(VirtualCommodityInventoryEntryEntity bareEntry);

    [MapProperty(nameof(CommodityInventoryEntryEntityBase.CommodityId), nameof(PhysicalCommodityInventoryEntry.Commodity), Use = nameof(ResolveCommodity))]
    [MapProperty(nameof(PhysicalCommodityInventoryEntryEntity.LocationId), nameof(PhysicalCommodityInventoryEntry.Location), Use = nameof(ResolveLocation))]
    private partial PhysicalCommodityInventoryEntry Map(PhysicalCommodityInventoryEntryEntity bareEntry);

    #endregion
}
