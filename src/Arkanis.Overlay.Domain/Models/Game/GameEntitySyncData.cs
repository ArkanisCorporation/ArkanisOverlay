namespace Arkanis.Overlay.Domain.Models.Game;

using Abstractions.Game;

public abstract record GameEntitySyncData<T> where T : class, IGameEntity;

public sealed record SyncDataUpToDate<T> : GameEntitySyncData<T> where T : class, IGameEntity;

public sealed record MissingSyncData<T> : GameEntitySyncData<T> where T : class, IGameEntity
{
    public static readonly GameEntitySyncData<T> Instance = new MissingSyncData<T>();

    private MissingSyncData()
    {
    }
}

public sealed record LoadedSyncData<T>(IAsyncEnumerable<T> GameEntities, DataCached DataState)
    : GameEntitySyncData<T> where T : class, IGameEntity;
