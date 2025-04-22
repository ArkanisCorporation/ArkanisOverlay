namespace Arkanis.Overlay.Infrastructure.Repositories.Local;

using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Services;

internal class GameEntityAggregateRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IEnumerable<IGameEntityRepository> gameEntityRepositories
) : IGameEntityAggregateRepository
{
    public async IAsyncEnumerable<IGameEntity> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var outputChannel = Channel.CreateBounded<IGameEntity>(10);

        async Task PassToOutputAsync(IAsyncEnumerable<IGameEntity> entities)
            => await entities.ForEachAwaitAsync(async entity => await outputChannel.Writer.WriteAsync(entity, cancellationToken), cancellationToken);

        var processingTasks = gameEntityRepositories
            .Select(repository => repository.GetAllAsync(cancellationToken))
            .Select(PassToOutputAsync);

        var finalizeTask = Task.WhenAll(processingTasks)
            .ContinueWith(
                async task =>
                {
                    try
                    {
                        await task;
                    }
                    finally
                    {
                        outputChannel.Writer.Complete();
                    }
                },
                cancellationToken
            );

        await foreach (var gameEntity in outputChannel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return gameEntity;
        }

        await finalizeTask;
    }

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await DependencyResolver.DependsOn(this, gameEntityRepositories)
            .WaitUntilReadyAsync(cancellationToken)
            .ConfigureAwait(false);
}
