namespace Arkanis.Overlay.Infrastructure.Repositories.Local.Specialised;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;
using Services;

internal abstract class RepositorySpecialisationDecoratorBase<T>(IGameEntityRepository<T> decoratedRepository)
    : InitializableBase, IGameEntityRepository<T>, IDecoratorService
    where T : class, IGameEntity
{
    protected IGameEntityRepository<T> DecoratedRepository
        => decoratedRepository;

    public InternalDataState DataState
        => decoratedRepository.DataState;

    public async Task UpdateAllAsync(GameEntitySyncData<T> syncData, CancellationToken cancellationToken = default)
    {
        try
        {
            await DecoratedRepository.UpdateAllAsync(syncData, cancellationToken);
            await UpdateAllAsyncCore(cancellationToken);
            Initialized();
        }
        catch (Exception ex)
        {
            {
                InitializationErrored(ex);
            }
        }
    }

    public IAsyncEnumerable<T> GetAllAsync(CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAllAsync(cancellationToken);

    public Task<T?> GetAsync(IDomainId id, CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAsync(id, cancellationToken);

    protected abstract Task UpdateAllAsyncCore(CancellationToken cancellationToken);
}
