namespace Arkanis.Overlay.Infrastructure.Repositories.Local;

using System.Runtime.CompilerServices;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;

public class UexGameEntityInMemoryRepository<T> : IGameEntityRepository<T> where T : class, IGameEntity
{
    private readonly TaskCompletionSource _initialization = new();

    internal DateTimeOffset CachedUntil { get; set; } = DateTimeOffset.UtcNow;
    internal GameDataState CurrentDataState { get; set; } = MissingGameDataState.Instance;
    internal Dictionary<UexApiGameEntityId, T> Entities { get; set; } = [];

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => _initialization.Task;

    public async IAsyncEnumerable<T> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await WaitUntilReadyAsync(cancellationToken);
        foreach (var gameEntity in Entities.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return gameEntity;
        }
    }

    public Task<T?> GetAsync(IDomainId id, CancellationToken cancellationToken = default)
        => id is UexApiGameEntityId uexApiId
            ? Task.FromResult(Entities.GetValueOrDefault(uexApiId))
            : Task.FromResult<T?>(null);

    public ValueTask<AppDataState> GetDataStateAsync(GameDataState gameDataState, CancellationToken cancellationToken = default)
    {
        AppDataState state = CurrentDataState switch
        {
            MissingGameDataState => new AppDataMissing(),
            SyncedGameDataState current => gameDataState switch
            {
                SyncedGameDataState external => current.Version == external.Version && external.UpdatedAt < CachedUntil
                    ? new AppDataUpToDate(current)
                    : new AppDataOutOfDate(current),
                _ => new AppDataUpToDate(current),
            },
            _ => throw new NotSupportedException($"Unable to determine app data state from current game data state: {CurrentDataState}"),
        };
        return ValueTask.FromResult(state);
    }

    public async Task UpdateAllAsync(GameEntitySyncData<T> syncData, CancellationToken cancellationToken = default)
    {
        try
        {
            CurrentDataState = syncData.DataState;
            CachedUntil = syncData.CacheUntil;
            Entities = await syncData.GameEntities.ToDictionaryAsync(x => x.Id, cancellationToken);
            _initialization.TrySetResult();
        }
        catch (Exception ex)
        {
            _initialization.TrySetException(ex);
            throw;
        }
    }
}
