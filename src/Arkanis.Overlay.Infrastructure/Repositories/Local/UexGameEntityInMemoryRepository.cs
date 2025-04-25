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

    internal Dictionary<UexApiGameEntityId, T> Entities { get; set; } = [];
    public InternalDataState DataState { get; private set; } = DataMissing.Instance;

    public bool IsReady
        => _initialization.Task.IsCompletedSuccessfully;

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

    public async Task UpdateAllAsync(GameEntitySyncData<T> syncData, CancellationToken cancellationToken = default)
    {
        if (syncData is SyncDataUpToDate<T>)
        {
            logger.LogDebug("Skipping update for {EntityType}, already up to date", typeof(T).Name);
            return;
        }

        if (syncData is not LoadedSyncData<T> loadedSyncData)
        {
            logger.LogWarning("Skipping update for {EntityType}: {@SyncData}", typeof(T).Name, syncData);
            return;
        }

        try
        {
            DataState = loadedSyncData.DataState;
            Entities = await loadedSyncData.GameEntities.ToDictionaryAsync(x => x.Id, cancellationToken).ConfigureAwait(false);
            _initialization.TrySetResult();
            logger.LogInformation(
                "Repository updated successfully to {CurrentDataState} with {EntityCount} entities",
                DataState,
                Entities.Count
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update repository of {EntityType}", typeof(T).Name);
            _initialization.TrySetException(ex);
            throw;
        }
    }
}
