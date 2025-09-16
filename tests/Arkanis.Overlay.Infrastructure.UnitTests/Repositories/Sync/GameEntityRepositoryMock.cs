namespace Arkanis.Overlay.Infrastructure.UnitTests.Repositories.Sync;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Common.Services;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;

internal sealed class GameEntityRepositoryMock<T>(
    IGameEntityExternalSyncRepository<T> repository,
    ILogger<GameEntityRepositoryMock<T>> logger
) : InitializableBase, IGameEntityRepository<T>
    where T : class, IGameEntity
{
    internal List<T> Entities { get; set; } = [];

    public async IAsyncEnumerable<T> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var syncData = await repository.GetAllAsync(DataMissing.Instance, cancellationToken);
        if (syncData is not LoadedSyncData<T> loadedSyncData)
        {
            yield break;
        }

        await foreach (var gameEntity in loadedSyncData.GameEntities.WithCancellation(cancellationToken))
        {
            yield return gameEntity;
        }
    }

    public Task<T?> GetAsync(IDomainId id, CancellationToken cancellationToken = default)
        => repository.GetAsync(id, cancellationToken);

    public InternalDataState DataState
    {
        get
        {
            var gameVersion = StarCitizenVersion.Create("4.1.0");
            var serviceState = new ServiceAvailableState(gameVersion, DateTimeOffset.UtcNow);
            return new DataLoaded(serviceState, DateTimeOffset.UtcNow);
        }
    }

    public async Task UpdateAllAsync(GameEntitySyncData<T> syncData, CancellationToken cancellationToken = default)
    {
        if (syncData is not LoadedSyncData<T> loadedSyncData)
        {
            logger.LogDebug("Sync data is not loaded, skipping update");
            return;
        }

        try
        {
            Entities = await loadedSyncData.GameEntities.ToListAsync(cancellationToken);
            logger.LogDebug("Storing {EntityCount} loaded entities", Entities.Count);
            Initialized();
        }
        catch (Exception ex)
        {
            InitializationErrored(ex);
            throw;
        }
    }
}
