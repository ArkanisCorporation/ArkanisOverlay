namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models;

public interface IExternalSyncCacheProvider
{
    Task StoreAsync<TSource>(TSource source, DataCached dataState, CancellationToken cancellationToken = default);

    Task<SyncDataCache<TSource>> LoadAsync<TSource>(InternalDataState currentDataState, CancellationToken cancellationToken = default);
}

/// <summary>
///     In order to simplify the caching, generic type parameter is used as a primary key.
///     This results in any type having a unique cache.
/// </summary>
/// <remarks>
///     This cache is limited to a single value per unique inner type cached.
/// </remarks>
/// <typeparam name="TIdentity">Type used as a primary key for caching</typeparam>
public interface IExternalSyncCacheProvider<TIdentity> : IExternalSyncCacheProvider where TIdentity : class;
