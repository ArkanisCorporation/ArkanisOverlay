namespace Arkanis.Overlay.Infrastructure.Services;

using System.Text.Json;
using Data;
using Data.Entities;
using Data.Extensions;
using Domain.Abstractions.Services;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Repositories.Local;

/// <summary>
///     In order to simplify the caching, generic type parameter is used as a primary key.
///     That results in any type having a unique cache key component.
///     The primary key is a composite key, consisting of the repository type name and type name of the object stored.
/// </summary>
/// <remarks>
///     This cache provider is limited to a single value per unique inner type cached.
/// </remarks>
/// <typeparam name="TRepository">Type used as a primary key for caching</typeparam>
internal class ExternalSyncDatabaseCacheProvider<TRepository>(
    UexServiceStateProvider stateProvider,
    IDbContextFactory<OverlayDbContext> dbContextFactory,
    ILogger<ExternalSyncDatabaseCacheProvider<TRepository>> logger
) : IExternalSyncCacheProvider<TRepository> where TRepository : class
{
    private Type RepositoryType { get; } = typeof(TRepository);

    public async Task StoreAsync<TSource>(TSource source, InternalCacheProperties properties, CancellationToken cancellationToken = default)
    {
        var recordKey = CreateKey<TSource>();
        var dataState = properties.DataState;
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var cacheRecord = new ExternalSourceDataCache
        {
            Id = recordKey,
            Title = properties.Title,
            Description = properties.Description,
            Content = JsonSerializer.SerializeToDocument(source),
            ContentSizeBytes = JsonSerializer.SerializeToUtf8Bytes(source).LongLength,
            DataAvailableState = dataState.SourcedState,
            CachedUntil = dataState.CachedUntil,
        };
        await db.ExternalSourceDataCache.AddOrUpdateAsync(cacheRecord, cancellationToken);
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

        var currentServiceState = await stateProvider.LoadCurrentServiceStateAsync(cancellationToken);
        if (cacheRecord.CachedUntil < DateTimeOffset.UtcNow)
        {
            // the cached record has expired
            if (currentServiceState is ServiceUnavailableState)
            {
                logger.LogWarning(
                    "Permitting use of expired cache for {Type} entities due to the state of an external service: {ServiceState}",
                    typeof(TSource).ShortDisplayName(),
                    currentServiceState
                );
            }
            else if (currentDataState is DataProcessingErrored)
            {
                logger.LogWarning(
                    "Permitting use of expired cache for {Type} entities due to the current internal data state: {DataState}",
                    typeof(TSource).ShortDisplayName(),
                    currentDataState
                );
            }
            else
            {
                // only respect the cache time if the service is available and internal load state did not error
                return new ExpiredCache<TSource>(cacheRecord.CachedUntil);
            }
        }

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
