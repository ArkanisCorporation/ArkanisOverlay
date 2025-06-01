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
        return entities.Select(x => mapper.Map(x)).ToList();
    }

    public async Task UpdateEntryAsync(InventoryEntryBase entry, CancellationToken cancellationToken = default)
    {
        var entity = mapper.Map(entry);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.InventoryEntries.AddOrUpdateAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await CompactifyEntitiesAsync(entity, cancellationToken);
    }

    public async Task DeleteEntryAsync(InventoryEntryId entryId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.InventoryEntries
            .Where(x => x.Id == entryId)
            .IgnoreAutoIncludes()
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
        var currentListEntity = mapper.Map(list);
        var existingListEntity = await dbContext.InventoryLists
            .SingleOrDefaultAsync(x => x.Id == currentListEntity.Id, cancellationToken);

        if (existingListEntity is not null)
        {
            var existingEntryIds = existingListEntity.Entries.Select(x => x.Id).ToHashSet();
            var currentEntryIds = currentListEntity.Entries.Select(x => x.Id).ToHashSet();

            foreach (var entryEntity in existingListEntity.Entries.Where(entryEntity => !currentEntryIds.Contains(entryEntity.Id)))
            {
                //? these are entries newly missing from the current list
                entryEntity.List = null;
            }

            foreach (var entryEntity in currentListEntity.Entries.Where(entryEntity => !existingEntryIds.Contains(entryEntity.Id)))
            {
                //? these are entries newly added to the current list
                await dbContext.AddAsync(entryEntity, cancellationToken);
            }

            foreach (var entryEntity in currentListEntity.Entries.Where(entryEntity => existingEntryIds.Contains(entryEntity.Id)).ToList())
            {
                //? these are entries that were not changed
                currentListEntity.Entries.Remove(entryEntity);
            }

            dbContext.Entry(existingListEntity).State = EntityState.Detached;
            await dbContext.InventoryLists.AddOrUpdateAsync(currentListEntity, cancellationToken);
        }
        else
        {
            await dbContext.AddAsync(currentListEntity, cancellationToken);
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

    /// <summary>
    ///     Compacts multiple database entities into one.
    ///     This is used to prevent having multiple entries for the same entity in the same list within the same location.
    /// </summary>
    /// <param name="current">The entry entity to compactify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task CompactifyEntitiesAsync(InventoryEntryEntityBase current, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var otherExistingEntities = await dbContext.InventoryEntries
            .Where(x => x.Id != current.Id)
            .Where(x => x.ListId == current.ListId)
            .Where(x => x.GameEntityId == current.GameEntityId)
            .Where(x => x.Quantity.Unit == current.Quantity.Unit)
            .Where(x => x.Discriminator == current.Discriminator)
            .AsNoTracking()
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

        foreach (var other in otherExistingEntities)
        {
            current.Quantity.Amount += other.Quantity.Amount;
            other.List = null; //! prevent recursive change tracking of the list and its related entities
            dbContext.InventoryEntries.Remove(other);
        }

        dbContext.InventoryEntries.Update(current);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
