namespace Arkanis.Overlay.Domain.Models;

public abstract record CachedSyncData<TData>;

public sealed record MissingData<TData> : CachedSyncData<TData>;

public sealed record ExpiredCache<TData>(DateTimeOffset ExpiredAt) : CachedSyncData<TData>;

public sealed record UnprocessableData<TData> : CachedSyncData<TData>;

public sealed record LoadedSyncData<TData>(TData Data, AppDataCached State) : CachedSyncData<TData>;
