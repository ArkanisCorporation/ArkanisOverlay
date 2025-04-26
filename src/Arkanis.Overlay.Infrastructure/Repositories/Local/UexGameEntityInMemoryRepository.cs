namespace Arkanis.Overlay.Infrastructure.Repositories.Local;

using System.Runtime.CompilerServices;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;
using Microsoft.Extensions.Logging;
using Services;

public class UexGameEntityInMemoryRepository<T>(ILogger<UexGameEntityInMemoryRepository<T>> logger) : InitializableBase, IGameEntityRepository<T>
    where T : class, IGameEntity
{
    internal Dictionary<UexApiGameEntityId, T> Entities { get; set; } = [];
    public InternalDataState DataState { get; private set; } = DataMissing.Instance;

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
            logger.LogDebug("Skipping update for {EntityType} entities, already up to date", typeof(T).Name);
            return;
        }

        if (syncData is not LoadedSyncData<T> loadedSyncData)
        {
            logger.LogWarning("Unable to perform update of {EntityType} entities: {@SyncData}", typeof(T).Name, syncData);
            return;
        }

        try
        {
            DataState = loadedSyncData.DataState;
            Entities = await loadedSyncData.GameEntities.ToDictionaryAsync(x => x.Id, cancellationToken).ConfigureAwait(false);
            Initialized();
            logger.LogInformation(
                "Repository updated successfully to {CurrentDataState} with {EntityCount} entities",
                DataState,
                Entities.Count
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update repository of {EntityType}", typeof(T).Name);
            InitializationErrored(ex);
            throw;
        }
    }
}
