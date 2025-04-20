namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;

public interface IGameEntityReadOnlyRepository<T> where T : class, IGameEntity
{
    IAsyncEnumerable<T> GetAllAsync(CancellationToken cancellationToken = default);

    Task<T?> GetAsync(IGameEntityId id, CancellationToken cancellationToken = default);
}
