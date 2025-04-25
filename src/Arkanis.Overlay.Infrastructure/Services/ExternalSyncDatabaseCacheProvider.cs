namespace Arkanis.Overlay.Infrastructure.Services;

using System.Text.Json;
using Data;
using Data.Entities;
using Data.Extensions;
using Domain.Abstractions.Services;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Repositories.Local;

internal class ExternalSyncDatabaseCacheProvider<TRepository>(
    UexServiceStateProvider stateProvider,
    IDbContextFactory<OverlayDbContext> dbContextFactory
) : IExternalSyncCacheProvider<TRepository> where TRepository : class
{
    private Type RepositoryType { get; } = typeof(TRepository);

    public async Task StoreAsync<TSource>(TSource source, DataCached dataState, CancellationToken cancellationToken = default)
    {
        var recordKey = CreateKey<TSource>();
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var cacheRecord = new ExternalSourceDataCache
        {
            Id = recordKey,
            Content = JsonSerializer.SerializeToDocument(source),
            DataAvailableState = dataState.SourcedState,
            CachedUntil = dataState.CachedUntil,
        };
        db.ExternalSourceDataCache.AddOrUpdate(cacheRecord);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<SyncDataCache<TSource>> LoadAsync<TSource>(InternalDataState currentDataState, CancellationToken cancellationToken = default)
    {
        var recordKey = CreateKey<TSource>();
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var cacheRecord = await db.ExternalSourceDataCache.SingleOrDefaultAsync(record => record.Id == recordKey, cancellationToken);
        if (cacheRecord is null)
        {
            // there is no cached record in the database
            return new MissingDataCache<TSource>();
        }

        if (cacheRecord.CachedUntil < DateTimeOffset.UtcNow)
        {
            // the cached record has expired
            return new ExpiredCache<TSource>(cacheRecord.CachedUntil);
        }

        var currentServiceState = await stateProvider.LoadCurrentServiceStateAsync(cancellationToken);
        var cachedServiceState = cacheRecord.DataAvailableState;
        var cachedDataState = new DataCached(cachedServiceState, cachedServiceState.UpdatedAt, cacheRecord.CachedUntil);

        if (currentDataState is DataLoaded loadedData)
        {
            if (currentServiceState is ServiceAvailableState availableState)
            {
                if (loadedData.SourcedState.Version != availableState.Version)
                {
                    // the repository already has some data, but the sourced game version does not match
                    return new ExpiredCache<TSource>(cacheRecord.CachedUntil);
                }

                // the repository already has some data, cache did not expire yet and the sourced game version matches
                return new AlreadyUpToDateWithCache<TSource>(cachedDataState);
            }
        }

        if (cacheRecord.Content.Deserialize<TSource>() is not { } cachedData)
        {
            // the cached data is not valid
            return new UnprocessableDataCache<TSource>();
        }

        // the cached data is needed
        return new LoadedSyncDataCache<TSource>(cachedData, cachedDataState);
    }

    private string CreateKey<TSource>()
        => $"{RepositoryType.ShortDisplayName()}-{typeof(TSource).ShortDisplayName()}";
}
