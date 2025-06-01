namespace Arkanis.Overlay.Infrastructure.Services;

using Data;
using Data.Entities;
using Data.Entities.Abstractions;
using Data.Extensions;
using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Inventory;
using Microsoft.EntityFrameworkCore;

internal class LocalDatabaseInventoryManager(
    IDbContextFactory<OverlayDbContext> dbContextFactory,
    InventoryEntityMapper mapper
) : IInventoryManager
{
    public async Task<ICollection<InventoryEntryBase>> GetAllEntriesAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await dbContext.InventoryEntries.ToListAsync(cancellationToken);
        return entities.Select(mapper.Map).ToList();
    }

    public async Task UpdateEntryAsync(InventoryEntryBase entry, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = mapper.Map(entry);
        await dbContext.InventoryEntries.AddOrUpdateAsync(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await CompactifyEntitiesAsync(entity, cancellationToken);
    }

    public async Task DeleteEntryAsync(InventoryEntryId entryId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.InventoryEntries
            .Where(x => x.Id == entryId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<InventoryEntryList?> GetListAsync(InventoryEntryListId listId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.InventoryLists.SingleOrDefaultAsync(x => x.Id == listId, cancellationToken);
        return entity is not null
            ? mapper.Map(entity)
            : null;
    }

    public async Task<ICollection<InventoryEntryList>> GetAllListsAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await dbContext.InventoryLists.ToListAsync(cancellationToken);
        return entities.Select(mapper.Map).ToList();
    }

    public async Task UpdateListAsync(InventoryEntryList list, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var alreadyExists = await dbContext.InventoryLists.AnyAsync(x => x.Id == list.Id, cancellationToken);

        var listEntity = mapper.Map(list);
        if (alreadyExists)
        {
            var existingList = await dbContext.InventoryLists
                .AsNoTracking()
                .SingleAsync(x => x.Id == list.Id, cancellationToken);

            var removedEntryIds = existingList.Entries
                .Where(existing => listEntity.Entries.All(current => current.Id != existing.Id))
                .Select(x => x.Id)
                .ToArray();

            await dbContext.InventoryListItems
                .Where(x => x.ListId == existingList.Id)
                .Where(x => removedEntryIds.Contains(x.EntryId))
                .ExecuteDeleteAsync(cancellationToken);

            var newEntries = listEntity.Entries
                .Where(x => existingList.Entries.All(y => y.Id != x.Id))
                .ToList();

            await dbContext.InventoryEntries.AddRangeAsync(newEntries, cancellationToken);

            var newListItems = newEntries.Select(x => new InventoryEntryListItemEntity
                {
                    ListId = list.Id,
                    EntryId = x.Id,
                }
            );
            await dbContext.InventoryListItems.AddRangeAsync(newListItems, cancellationToken);

            foreach (var entry in listEntity.Entries)
            {
                await dbContext.InventoryEntries.AddOrUpdateAsync(entry);
            }

            listEntity.Entries.Clear();
            await dbContext.InventoryLists.AddOrUpdateAsync(listEntity);
        }
        else
        {
            await dbContext.AddAsync(listEntity, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteListAsync(InventoryEntryListId listId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.InventoryLists
            .Where(x => x.Id == listId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task CompactifyEntitiesAsync(InventoryEntryEntityBase current, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var otherExistingEntities = await dbContext.InventoryEntries
            .Where(x => x.Id != current.Id)
            .Where(x => x.GameEntityId == current.GameEntityId)
            .Where(x => x.Quantity.Unit == current.Quantity.Unit)
            .Where(x => x.Discriminator == current.Discriminator)
            .ToListAsync(cancellationToken);

        if (otherExistingEntities.Count == 0)
        {
            return;
        }

        if (current is IDatabaseEntityWithLocation currentAt)
        {
            // only merge entries at the same location
            otherExistingEntities.RemoveAll(other
                => other is not IDatabaseEntityWithLocation otherAt
                   || otherAt.LocationId != currentAt.LocationId
            );
        }

        foreach (var existing in otherExistingEntities)
        {
            current.Quantity.Amount += existing.Quantity.Amount;
            dbContext.InventoryEntries.Remove(existing);
        }

        dbContext.InventoryEntries.Update(current);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
