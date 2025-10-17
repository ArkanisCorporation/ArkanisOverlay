namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;

public interface IGameEntityReadOnlyRepository<T> where T : class, IGameEntity
{
    public IAsyncEnumerable<T> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<T?> GetAsync(IDomainId id, CancellationToken cancellationToken = default);
}
