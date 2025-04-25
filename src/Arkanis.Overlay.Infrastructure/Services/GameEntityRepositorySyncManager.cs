namespace Arkanis.Overlay.Infrastructure.Services;

using Abstractions;
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
) : SelfInitializableServiceBase, ISelfUpdatable where T : class, IGameEntity
{
    public async Task UpdateAsync(CancellationToken cancellationToken)
        => await UpdateAsync(false, cancellationToken);

    public async Task UpdateIfNecessaryAsync(CancellationToken cancellationToken)
        => await UpdateAsync(true, cancellationToken);

    private async Task UpdateAsync(bool onlyWhenNecessary, CancellationToken cancellationToken)
    {
        var currentDataState = repository.DataState;
        if (currentDataState is not DataMissing and DataCached { RefreshRequired: true } && onlyWhenNecessary)
        {
            logger.LogDebug("Current data of {EntityType} repository are up to date: {AppDataState}", typeof(T), currentDataState);
            return;
        }

        var syncData = await syncRepository.GetAllAsync(currentDataState, cancellationToken);
        logger.LogDebug(
            "Updating data of {EntityType} repository: {CurrentAppDataState} using {NewAppDataState}",
            typeof(T),
            currentDataState,
            syncData
        );

        await repository.UpdateAllAsync(syncData, cancellationToken);
    }

    protected override Task InitializeAsyncCore(CancellationToken cancellationToken)
        => UpdateAsync(true, cancellationToken);
}
