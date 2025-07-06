namespace Arkanis.Overlay.Infrastructure.Repositories.Local.Specialised;

using Common.Services;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;

/// <summary>
///     A base class for all repository decorators.
///     Simplifies the implementation of the decorator pattern wrapping the other functionality of the decorated repository.
/// </summary>
/// <param name="decoratedRepository">The repository to be decorated</param>
/// <typeparam name="T">Target entity type</typeparam>
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
            InitializationErrored(ex);
            throw;
        }
    }

    public IAsyncEnumerable<T> GetAllAsync(CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAllAsync(cancellationToken);

    public Task<T?> GetAsync(IDomainId id, CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAsync(id, cancellationToken);

    /// <summary>
    ///     Forces the decorator to update its internal state.
    /// </summary>
    /// <remarks>
    ///     This method will always be called after the decorated repository has finished updating itself.
    /// </remarks>
    /// <param name="cancellationToken">A process cancellation token</param>
    /// <returns>A processing task that fulfills when the repository has updated its internal state</returns>
    protected abstract Task UpdateAllAsyncCore(CancellationToken cancellationToken);
}
