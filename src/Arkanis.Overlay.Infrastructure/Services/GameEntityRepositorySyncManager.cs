namespace Arkanis.Overlay.Infrastructure.Services;

using Abstractions;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Quartz;

/// <summary>
///     This service performs synchronization of internal and external game entity repositories.
/// </summary>
/// <param name="syncRepositories">External repositories</param>
/// <param name="repository">Internal repository</param>
/// <param name="logger">A logger</param>
/// <typeparam name="T">Internal entity domain type</typeparam>
internal sealed class GameEntityRepositorySyncManager<T>(
    IEnumerable<IGameEntityExternalSyncRepository<T>> syncRepositories,
    IGameEntityRepository<T> repository,
    ILogger<GameEntityRepositorySyncManager<T>> logger
) : SelfInitializableServiceBase, ISelfUpdatable where T : class, IGameEntity
{
    public ITrigger Trigger { get; } = TriggerBuilder.Create()
        .StartAt(DateTimeOffset.UtcNow + TimeSpan.FromMinutes(10))
        .WithSimpleSchedule(schedule => schedule.WithInterval(TimeSpan.FromMinutes(10)).RepeatForever())
        .Build();

    public async Task UpdateAsync(CancellationToken cancellationToken)
        => await UpdateAsync(false, cancellationToken);

    public async Task UpdateIfNecessaryAsync(CancellationToken cancellationToken)
        => await UpdateAsync(true, cancellationToken);

    private async Task UpdateAsync(bool onlyWhenNecessary, CancellationToken cancellationToken)
    {
        var currentDataState = repository.DataState;
        if (!onlyWhenNecessary && currentDataState is DataCached cached)
        {
            currentDataState = cached with { RefreshRequired = true };
        }

        if (currentDataState is not DataMissing and DataCached { RefreshRequired: true } && onlyWhenNecessary)
        {
            logger.LogDebug("Current data of {EntityType} repository are up to date: {AppDataState}", typeof(T), currentDataState);
            return;
        }

        if (!syncRepositories.Any())
        {
            logger.LogWarning("No external repositories found for entity: {EntityType}", typeof(T));
            await repository.UpdateAllAsync(new SyncDataUpToDate<T>(), cancellationToken);
            return;
        }

        var aggregateSyncData = GameEntitySyncData<T>.Missing;
        foreach (var syncRepository in syncRepositories)
        {
            var syncData = await syncRepository.GetAllAsync(currentDataState, cancellationToken);
            aggregateSyncData = aggregateSyncData.MergeWith(syncData);
        }

        logger.LogDebug(
            "Updating data of {RepositoryType}: {CurrentAppDataState} using {NewAppDataState}",
            repository.GetType().ShortDisplayName(),
            currentDataState,
            aggregateSyncData
        );

        await repository.UpdateAllAsync(aggregateSyncData, cancellationToken);
    }

    protected override Task InitializeAsyncCore(CancellationToken cancellationToken)
        => UpdateAsync(true, cancellationToken);
}
