namespace Arkanis.Overlay.Infrastructure.Services;

using Abstractions;
using Data;
using Data.Entities;
using Data.Entities.Abstractions;
using Data.Extensions;
using Data.Mappers;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using Domain.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

internal class LocalDatabaseInventoryManager(
    IDbContextFactory<OverlayDbContext> dbContextFactory,
    IMemoryCache memoryCache,
    IChangeTokenManager changeTokenManager,
    InventoryEntityMapper mapper
) : IInventoryManager
{
    public IChangeToken ChangeToken
        => changeTokenManager.GetChangeTokenFor<CacheId>();

    public async Task<int> GetUnassignedCountAsync(CancellationToken cancellationToken = default)
        => await memoryCache.GetOrCreateAsync(
            CacheId.UnassignedCountQuery,
            async entry =>
            {
                entry.AddExpirationToken(ChangeToken);
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));

                await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                return await dbContext.InventoryEntries.CountAsync(x => x.EntryType == InventoryEntryBase.EntryType.Virtual, cancellationToken);
            }
        );

    public async Task<ICollection<InventoryEntryBase>> GetEntriesForAsync(
        IDomainId domainId,
        InventoryEntryBase.EntryType entryType,
        CancellationToken cancellationToken = default
    )
        => await memoryCache.GetOrCreateAsync(
               new CacheId.GetMatchingEntries(domainId, entryType),
               async entry =>
               {
                   entry.AddExpirationToken(ChangeToken);
                   entry.SetSlidingExpiration(TimeSpan.FromHours(1));

                   if (domainId is not UexApiGameEntityId uexId)
                   {
                       return [];
                   }

                   await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                   var entities = await dbContext.InventoryEntries
                       .Where(x => x.Quantity.Reference.EntityId == uexId)
                       .Where(x => x.EntryType == entryType)
                       .ToArrayAsync(cancellationToken);

                   return entities
                       .Select(x => mapper.Map(x))
                       .ToArray();
               }
           )
           ?? [];

    public async Task<ICollection<InventoryEntryBase>> GetEntriesForAsync(IDomainId domainId, CancellationToken cancellationToken = default)
        => await memoryCache.GetOrCreateAsync(
               new CacheId.GetMatchingEntries(domainId, InventoryEntryBase.EntryType.Undefined),
               async entry =>
               {
                   entry.AddExpirationToken(ChangeToken);
                   entry.SetSlidingExpiration(TimeSpan.FromHours(1));

                   if (domainId is not UexApiGameEntityId uexId)
                   {
                       return [];
                   }

                   await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                   var entities = await dbContext.InventoryEntries
                       .Where(x => x.Quantity.Reference.EntityId == uexId)
                       .ToArrayAsync(cancellationToken);

                   return entities
                       .Select(x => mapper.Map(x))
                       .ToArray();
               }
           )
           ?? [];

    public async Task<ICollection<InventoryEntryBase>> GetAllEntriesAsync(CancellationToken cancellationToken = default)
        => await memoryCache.GetOrCreateAsync(
               CacheId.AllEntriesQuery,
               async entry =>
               {
                   entry.AddExpirationToken(ChangeToken);
                   entry.SetSlidingExpiration(TimeSpan.FromHours(1));

                   await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                   var entities = await dbContext.InventoryEntries.ToArrayAsync(cancellationToken);
                   return entities.Select(x => mapper.Map(x)).ToArray();
               }
           )
           ?? [];

    public async Task AddOrUpdateEntryAsync(InventoryEntryBase entry, CancellationToken cancellationToken = default)
    {
        var entity = mapper.Map(entry);
        await using (var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            //! cannot simply delete this entity using ExecuteDeleteAsync: https://github.com/dotnet/efcore/issues/32823
            // await dbContext.InventoryEntries.Where(x => x.Id == entry.Id).ExecuteDeleteAsync(cancellationToken);
            var existing = await dbContext.InventoryEntries.FindAsync([entry.Id], cancellationToken);
            if (existing is not null)
            {
                dbContext.Remove(existing);
            }

            await dbContext.InventoryEntries.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await CompactifyEntitiesAsync(entity, cancellationToken);
        await TriggerChangeAsync();
    }

    public async Task DeleteEntryAsync(InventoryEntryId entryId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.InventoryEntries
            .Where(x => x.Id == entryId)
            .IgnoreAutoIncludes()
            .ExecuteDeleteAsync(cancellationToken);

        await TriggerChangeAsync();
    }

    public async Task<InventoryEntryList?> GetListAsync(InventoryEntryListId listId, CancellationToken cancellationToken = default)
        => await memoryCache.GetOrCreateAsync(
            new CacheId.GetList(listId),
            async entry =>
            {
                entry.AddExpirationToken(ChangeToken);
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));

                await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                var entity = await dbContext.InventoryLists.SingleOrDefaultAsync(x => x.Id == listId, cancellationToken);
                return entity is not null
                    ? mapper.Map(entity)
                    : null;
            }
        );

    public async Task<ICollection<InventoryEntryList>> GetAllListsAsync(CancellationToken cancellationToken = default)
        => await memoryCache.GetOrCreateAsync(
               CacheId.AllListsQuery,
               async entry =>
               {
                   entry.AddExpirationToken(ChangeToken);
                   entry.SetSlidingExpiration(TimeSpan.FromHours(1));

                   await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                   var entities = await dbContext.InventoryLists.ToArrayAsync(cancellationToken);
                   return entities.Select(mapper.Map).ToArray();
               }
           )
           ?? [];

    public async Task AddOrUpdateListAsync(InventoryEntryList list, CancellationToken cancellationToken = default)
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

        await TriggerChangeAsync();
    }

    public async Task DeleteListAsync(InventoryEntryListId listId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.InventoryLists
            .Where(x => x.Id == listId)
            .ExecuteDeleteAsync(cancellationToken);

        await TriggerChangeAsync();
    }

    private async Task TriggerChangeAsync()
        => await changeTokenManager.TriggerChangeForAsync<CacheId>();

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
            .Where(x => x.Quantity.Reference.EntityId == current.Quantity.Reference.EntityId)
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

    private class CacheId
    {
        public static readonly GetCount UnassignedCountQuery = new(InventoryEntryBase.EntryType.Virtual);

        public static readonly GetAll AllEntriesQuery = new(typeof(InventoryEntryEntityBase));

        public static readonly GetAll AllListsQuery = new(typeof(InventoryEntryListEntity));

        public record GetCount(InventoryEntryBase.EntryType Type);

        public record GetAll(Type EntityType);

        public record GetMatchingEntries(IDomainId DomainId, InventoryEntryBase.EntryType Type) : GetAll(typeof(InventoryEntryEntityBase));

        public record GetList(InventoryEntryListId ListId);
    }
}
