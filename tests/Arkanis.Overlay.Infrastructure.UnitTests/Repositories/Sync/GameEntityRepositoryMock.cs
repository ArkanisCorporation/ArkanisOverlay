namespace Arkanis.Overlay.Infrastructure.UnitTests.Repositories.Sync;

using System.Runtime.CompilerServices;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;

internal class GameEntityRepositoryMock<T>(IGameEntityExternalSyncRepository<T> repository) : IGameEntityRepository<T> where T : class, IGameEntity
{
    private readonly TaskCompletionSource _initialization = new();

    internal GameDataState CurrentDataState { get; private set; } = MissingGameDataState.Instance;
    internal List<T> Entities { get; set; } = [];

    public async IAsyncEnumerable<T> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var syncData = await repository.GetAllAsync(cancellationToken);
        await foreach (var gameEntity in syncData.GameEntities.WithCancellation(cancellationToken))
        {
            yield return gameEntity;
        }
    }

    public Task<T?> GetAsync(IDomainId id, CancellationToken cancellationToken = default)
        => repository.GetAsync(id, cancellationToken);

    public ValueTask<AppDataState> GetDataStateAsync(GameDataState gameDataState, CancellationToken cancellationToken = default)
        => Entities.Count == 0
            ? ValueTask.FromResult(AppDataMissing.Instance)
            : ValueTask.FromResult<AppDataState>(new AppDataUpToDate(gameDataState));

    public async Task UpdateAllAsync(GameEntitySyncData<T> syncData, CancellationToken cancellationToken = default)
    {
        try
        {
            Entities = await syncData.GameEntities.ToListAsync(cancellationToken);
            _initialization.TrySetResult();
        }
        catch (Exception ex)
        {
            _initialization.TrySetException(ex);
            throw;
        }
    }

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => _initialization.Task.WaitAsync(cancellationToken);
}
