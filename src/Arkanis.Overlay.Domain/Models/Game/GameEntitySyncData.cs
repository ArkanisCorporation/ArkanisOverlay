namespace Arkanis.Overlay.Domain.Models.Game;

using Abstractions.Game;

public abstract record GameEntitySyncData<T> where T : class, IGameEntity
{
    public static readonly GameEntitySyncData<T> Missing = MissingSyncData<T>.Instance;

    public GameEntitySyncData<T> MergeWith(GameEntitySyncData<T> other)
        => this switch
        {
            LoadedSyncData<T> loaded => other switch
            {
                LoadedSyncData<T> otherLoaded => new LoadedSyncData<T>(
                    loaded.GameEntities.Union(otherLoaded.GameEntities),
                    loaded.DataState // TODO: Merge / Select of the two
                ),
                SyncDataUpToDate<T> otherUpToDate => new LoadedSyncData<T>(
                    loaded.GameEntities.Union(otherUpToDate.GameEntities),
                    loaded.DataState // TODO: Merge / Select of the two
                ),
                _ => this,
            },
            SyncDataUpToDate<T> upToDate => other switch
            {
                LoadedSyncData<T> otherLoaded => otherLoaded.MergeWith(upToDate),
                SyncDataUpToDate<T> otherUpToDate => new SyncDataUpToDate<T>(
                    otherUpToDate.GameEntities.Union(otherUpToDate.GameEntities)
                ),
                _ => this,
            },
            _ => other switch
            {
                LoadedSyncData<T> otherLoaded => otherLoaded,
                SyncDataUpToDate<T> otherUpToDate => otherUpToDate,
                _ => this,
            },
        };
}

public sealed record SyncDataUpToDate<T>(IAsyncEnumerable<T> GameEntities) : GameEntitySyncData<T> where T : class, IGameEntity;

public sealed record MissingSyncData<T> : GameEntitySyncData<T> where T : class, IGameEntity
{
    public static readonly GameEntitySyncData<T> Instance = new MissingSyncData<T>();

    private MissingSyncData()
    {
    }
}

public sealed record LoadedSyncData<T>(IAsyncEnumerable<T> GameEntities, DataCached DataState)
    : GameEntitySyncData<T> where T : class, IGameEntity;
