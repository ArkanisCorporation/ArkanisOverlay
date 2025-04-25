namespace Arkanis.Overlay.Infrastructure.Repositories.Local.Specialised;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;

internal abstract class RepositorySpecialisationDecoratorBase<T>(IGameEntityRepository<T> decoratedRepository)
    : IGameEntityRepository<T>, IDecoratorService
    where T : class, IGameEntity
{
    private readonly TaskCompletionSource _initialization = new();

    protected IGameEntityRepository<T> DecoratedRepository
        => decoratedRepository;

    public bool IsReady
        => _initialization.Task.IsCompleted;

    public InternalDataState DataState
        => decoratedRepository.DataState;

    public async Task UpdateAllAsync(GameEntitySyncData<T> syncData, CancellationToken cancellationToken = default)
    {
        try
        {
            await DecoratedRepository.UpdateAllAsync(syncData, cancellationToken);
            await UpdateAllAsyncCore(cancellationToken);
            _initialization.TrySetResult();
        }
        catch (Exception ex)
        {
            _initialization.TrySetException(ex);
        }
    }

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
    {
        await DecoratedRepository.WaitUntilReadyAsync(cancellationToken);
        await _initialization.Task;
    }

    public IAsyncEnumerable<T> GetAllAsync(CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAllAsync(cancellationToken);

    public Task<T?> GetAsync(IDomainId id, CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAsync(id, cancellationToken);

    protected abstract Task UpdateAllAsyncCore(CancellationToken cancellationToken);
}
