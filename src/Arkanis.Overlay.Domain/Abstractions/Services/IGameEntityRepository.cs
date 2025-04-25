namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models;
using Models.Game;

/// <summary>
///     A repository interface matching for all game entities.
///     Necessary for <see cref="IGameEntityAggregateRepository" />.
/// </summary>
public interface IGameEntityRepository : IGameEntityReadOnlyRepository<IGameEntity>, IDependable
{
    Type EntityType { get; }
}

/// <summary>
///     A generic repository for a specific game entity type.
///     This type cannot include <see cref="IGameEntityRepository" /> itself as that would cause method call ambiguity.
/// </summary>
/// <typeparam name="T">Game entity type</typeparam>
public interface IGameEntityRepository<T> : IGameEntityReadOnlyRepository<T>, IDependable
    where T : class, IGameEntity
{
    InternalDataState DataState { get; }

    Task UpdateAllAsync(GameEntitySyncData<T> syncData, CancellationToken cancellationToken = default);
}
