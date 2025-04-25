namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models;
using Models.Game;

/// <summary>
///     Represents an external source of game entity data.
///     These services will be used to populate the fully-fledged repository (potentially allowing improved filtering
///     capabilities).
/// </summary>
/// <typeparam name="T">The entity model provided</typeparam>
public interface IGameEntityExternalSyncRepository<T> where T : class, IGameEntity
{
    ValueTask<AppDataState> CreateAppDataStateFor(GameDataState localDataState, CancellationToken cancellationToken = default);

    ValueTask<GameEntitySyncData<T>> GetAllAsync(AppDataState appDataState, CancellationToken cancellationToken = default);

    Task<T?> GetAsync(IDomainId id, CancellationToken cancellationToken = default);
}
