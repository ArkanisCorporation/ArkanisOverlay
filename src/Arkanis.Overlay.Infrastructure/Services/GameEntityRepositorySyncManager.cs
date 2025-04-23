namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Microsoft.Extensions.Logging;

/// <summary>
///     This service performs synchronization of internal and external game entity repositories.
/// </summary>
/// <param name="syncRepository">External repository</param>
/// <param name="repository">Internal repository</param>
/// <param name="logger">A logger</param>
/// <typeparam name="T">Internal entity domain type</typeparam>
internal sealed class GameEntityRepositorySyncManager<T>(
    IGameEntityExternalSyncRepository<T> syncRepository,
    IGameEntityRepository<T> repository,
    ILogger<GameEntityRepositorySyncManager<T>> logger
) where T : class, IGameEntity
{
    public async Task UpdateIfNecessaryAsync(CancellationToken cancellationToken)
    {
        var gameDataState = await syncRepository.LoadCurrentDataState(cancellationToken);
        var currentLocalState = await repository.GetDataStateAsync(gameDataState, cancellationToken);
        if (currentLocalState is AppDataUpToDate)
        {
            // TODO: Initialize mapper cache if empty
            logger.LogDebug("Current data of {EntityType} repository are up to date: {AppDataState}", typeof(T), currentLocalState);
            return;
        }

        var syncData = await syncRepository.GetAllAsync(cancellationToken);
        logger.LogDebug(
            "Updating data of {EntityType} repository: {CurrentAppDataState} -> {NewAppDataState}",
            typeof(T),
            currentLocalState,
            syncData.DataState
        );

        await repository.UpdateAllAsync(syncData, cancellationToken);
    }
}
