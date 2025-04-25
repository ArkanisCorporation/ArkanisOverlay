namespace Arkanis.Overlay.Infrastructure.Repositories.Local;

using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using Services;

internal class GameEntityAggregateRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IEnumerable<IGameEntityRepository> gameEntityRepositories
) : IGameEntityAggregateRepository
{
    private readonly Type[] ExcludedEntityTypes = [typeof(GameEntityPricing), typeof(GameItemTrait), typeof(GameProductCategory)];

    public async IAsyncEnumerable<IGameEntity> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var outputChannel = Channel.CreateBounded<IGameEntity>(10);

        async Task PassToOutputAsync(IAsyncEnumerable<IGameEntity> entities)
            => await entities.ForEachAwaitAsync(async entity => await outputChannel.Writer.WriteAsync(entity, cancellationToken), cancellationToken);

        var processingTasks = gameEntityRepositories
            .Where(repository => !ExcludedEntityTypes.Any(excludedType => repository.EntityType.IsAssignableTo(excludedType)))
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

    public bool IsReady { get; private set; }

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await dependencyResolver.DependsOn(this, gameEntityRepositories)
            .WaitUntilReadyAsync(cancellationToken)
            .ContinueWith(_ => IsReady = true, cancellationToken)
            .ConfigureAwait(false);
}
