namespace Arkanis.Overlay.Infrastructure.Repositories.Local;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;

/// <summary>
///     Wraps a specific game entity repository to allow using it as a generic game entity repository.
/// </summary>
/// <param name="repository">Repository for the selected game entity</param>
/// <typeparam name="T">Corresponding game entity type</typeparam>
public class UexGameEntityGenericRepositoryAdapter<T>(IGameEntityRepository<T> repository) : IGameEntityRepository
    where T : class, IGameEntity
{
    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => repository.WaitUntilReadyAsync(cancellationToken);

    public IAsyncEnumerable<IGameEntity> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);

    public async Task<IGameEntity?> GetAsync(IDomainId id, CancellationToken cancellationToken = default)
        => await repository.GetAsync(id, cancellationToken);
}
