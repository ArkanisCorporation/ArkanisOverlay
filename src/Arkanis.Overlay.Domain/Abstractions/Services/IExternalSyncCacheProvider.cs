namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models;

public interface IExternalSyncCacheProvider
{
    public Task StoreAsync<TSource>(TSource source, InternalCacheProperties properties, CancellationToken cancellationToken = default);

    public Task<SyncDataCache<TSource>> LoadAsync<TSource>(InternalDataState currentDataState, CancellationToken cancellationToken = default);
}

public interface IExternalSyncCacheProvider<TIdentity> : IExternalSyncCacheProvider where TIdentity : class;
