namespace Arkanis.Overlay.Domain.Models;

public abstract record SyncDataCache<TData>;

public sealed record MissingDataCache<TData> : SyncDataCache<TData>;

public sealed record ExpiredCache<TData>(DateTimeOffset ExpiredAt) : SyncDataCache<TData>;

public sealed record UnprocessableDataCache<TData> : SyncDataCache<TData>;

public sealed record AlreadyUpToDateWithCache<TData>(DataCached State) : SyncDataCache<TData>;

public sealed record LoadedSyncDataCache<TData>(TData Data, DataCached State) : SyncDataCache<TData>;
