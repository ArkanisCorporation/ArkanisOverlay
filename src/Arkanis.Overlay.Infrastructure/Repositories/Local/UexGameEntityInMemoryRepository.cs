namespace Arkanis.Overlay.Infrastructure.Repositories.Local;

using System.Runtime.CompilerServices;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;
using Microsoft.Extensions.Logging;

public class UexGameEntityInMemoryRepository<T>(ILogger<UexGameEntityInMemoryRepository<T>> logger) : IGameEntityRepository<T> where T : class, IGameEntity
{
    private readonly TaskCompletionSource _initialization = new();

    internal DateTimeOffset CachedUntil { get; set; } = DateTimeOffset.UtcNow;
    internal GameDataState CurrentDataState { get; set; } = MissingGameDataState.Instance;
    internal Dictionary<UexApiGameEntityId, T> Entities { get; set; } = [];

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await _initialization.Task.WaitAsync(cancellationToken);

    public async IAsyncEnumerable<T> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await WaitUntilReadyAsync(cancellationToken).ConfigureAwait(false);
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
            Entities = await syncData.GameEntities.ToDictionaryAsync(x => x.Id, cancellationToken).ConfigureAwait(false);
            _initialization.TrySetResult();
            logger.LogInformation(
                "Repository updated successfully to {CurrentDataState} with {EntityCount} entities cached until {CachedUntil}",
                CurrentDataState,
                CachedUntil,
                Entities.Count
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update repository");
            _initialization.TrySetException(ex);
            throw;
        }
    }
}
