namespace Arkanis.Overlay.Domain.Models;

public static class SyncDataCache
{
    public static SyncDataCache<T> Missing<T>()
        => new MissingDataCache<T>();

    public static SyncDataCache<T> Unprocessable<T>()
        => new UnprocessableDataCache<T>(null);
}

public abstract record SyncDataCache<TData>;

public sealed record MissingDataCache<TData> : SyncDataCache<TData>;

public sealed record ExpiredCache<TData>(DateTimeOffset ExpiredAt) : SyncDataCache<TData>;

public sealed record UnprocessableDataCache<TData>(Exception? Exception) : SyncDataCache<TData>;

public sealed record AlreadyUpToDateWithCache<TData>(TData Data, DataCached State) : SyncDataCache<TData>;

public sealed record LoadedSyncDataCache<TData>(TData Data, DataCached State) : SyncDataCache<TData>;
