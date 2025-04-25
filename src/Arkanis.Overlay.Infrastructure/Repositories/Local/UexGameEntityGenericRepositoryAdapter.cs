namespace Arkanis.Overlay.Infrastructure.Repositories.Local;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;

/// <summary>
///     Wraps a specific game entity repository to allow using it as a generic game entity repository.
/// </summary>
/// <param name="repository">Repository for the selected game entity</param>
/// <typeparam name="T">Corresponding game entity type</typeparam>
public class UexGameEntityGenericRepositoryAdapter<T>(IGameEntityRepository<T> repository) : IGameEntityRepository
    where T : class, IGameEntity
{
    public Type EntityType { get; } = typeof(T);

    public InternalDataState DataState
        => repository.DataState;

    public bool IsReady
        => repository.IsReady;

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await repository.WaitUntilReadyAsync(cancellationToken).ConfigureAwait(false);

    public IAsyncEnumerable<IGameEntity> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);

    public async Task<IGameEntity?> GetAsync(IDomainId id, CancellationToken cancellationToken = default)
        => await repository.GetAsync(id, cancellationToken);
}
