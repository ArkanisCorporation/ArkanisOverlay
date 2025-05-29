namespace Arkanis.Overlay.Infrastructure.Data.Mappers;

using Domain.Abstractions.Game;
using Domain.Models.Game;
using Domain.Models.Inventory;
using Entities;
using Riok.Mapperly.Abstractions;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal partial class InventoryEntityMapper(UexApiDtoMapper uexMapper)
{
    public InventoryEntryEntityBase Map(InventoryEntryBase entryBase)
        => entryBase switch
        {
            // items
            VirtualItemInventoryEntry entry => Map(entry),
            PhysicalItemInventoryEntry entry => Map(entry),
            // others
            _ => throw new NotSupportedException($"Database entity mapping not supported for source domain object: {entryBase}"),
        };

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityId))]
    private partial VirtualItemInventoryEntryEntity Map(VirtualItemInventoryEntry bareEntry);

    [MapperIgnoreTarget(nameof(InventoryEntryEntityBase.GameEntityId))]
    private partial PhysicalItemInventoryEntryEntity Map(PhysicalItemInventoryEntry bareEntry);

    public InventoryEntryBase Map(InventoryEntryEntityBase entryBase)
        => entryBase switch
        {
            // items
            VirtualItemInventoryEntryEntity entry => Map(entry),
            PhysicalItemInventoryEntryEntity entry => Map(entry),
            // others
            _ => throw new NotSupportedException($"Domain entity mapping not supported for source database object: {entryBase}"),
        };

    [MapProperty(nameof(ItemInventoryEntryEntityBase.ItemId), nameof(VirtualItemInventoryEntry.Item), Use = nameof(ResolveItem))]
    private partial VirtualItemInventoryEntry Map(VirtualItemInventoryEntryEntity bareEntry);

    [MapProperty(nameof(ItemInventoryEntryEntityBase.ItemId), nameof(PhysicalItemInventoryEntry.Item), Use = nameof(ResolveItem))]
    [MapProperty(nameof(PhysicalItemInventoryEntryEntity.LocationId), nameof(PhysicalItemInventoryEntry.Location), Use = nameof(ResolveLocation))]
    private partial PhysicalItemInventoryEntry Map(PhysicalItemInventoryEntryEntity bareEntry);

    private GameItem ResolveItem(UexId<GameItem> itemId)
        => uexMapper.ResolveCachedGameEntity(itemId);

    private IGameLocation ResolveLocation(UexApiGameEntityId locationId)
        => uexMapper.ResolveCachedGameEntity<IGameLocation>(locationId);
}
