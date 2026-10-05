namespace Arkanis.Overlay.Infrastructure.Services;

using Common.Abstractions.Services;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
///     Provides support logic for resolving multiple dependencies at once.
/// </summary>
/// <param name="serviceProvider">Service provider used for typed service resolution</param>
/// <param name="logger">A logger</param>
internal sealed class GameEntityRepositoryDependencyResolver(
    IServiceProvider serviceProvider,
    ILogger<GameEntityRepositoryDependencyResolver> logger
) : DependencyResolver(serviceProvider)
{
    private readonly ILogger<GameEntityRepositoryDependencyResolver> _logger = logger;

    public new Context DependsOn(object dependent, params IEnumerable<IDependable> dependencies)
        => CreateDependencyContext(dependent, dependencies);

    public Context DependsOn<T>(object dependent) where T : class, IGameEntity
        => CreateDependencyContextOnGameEntity<T>(dependent);

    public Context DependsOn(object dependent, Type gameEntityType)
        => CreateDependencyContextOnGameEntity(dependent, gameEntityType);

    private Context CreateDependencyContextOnGameEntity<T>(object dependent) where T : class, IGameEntity
        => CreateDependencyContext(dependent, CreateDependencyOn<IGameEntityRepository<T>>());

    private Context CreateDependencyContextOnGameEntity(object dependent, Type gameEntityType)
        => CreateDependencyContext(dependent, CreateDependencyOnGameEntity(gameEntityType));

    private IDependable CreateDependencyOnGameEntity(Type gameEntityType)
    {
        var repositoryType = typeof(IGameEntityRepository<>).MakeGenericType(gameEntityType);
        var service = ServiceProvider.GetRequiredService(repositoryType);
        var repository = service as IDependable ?? throw new InvalidOperationException($"failed to resolve {repositoryType} for {gameEntityType}.");
        return repository;
    }

    private Context CreateDependencyContext(object dependent, params IEnumerable<IDependable> dependencies)
        => new(dependent, this, dependencies);

    public new sealed class Context(object dependent, GameEntityRepositoryDependencyResolver resolver, params IEnumerable<IDependable> dependencies)
        : DependencyResolver.Context(dependent, resolver._logger, dependencies)
    {
        public Context AlsoDependsOn<T>() where T : class, IGameEntity
        {
            AddDependency(resolver.CreateDependencyOn<IGameEntityRepository<T>>());
            return this;
        }

        public Context AlsoDependsOn(Type gameEntityType)
        {
            AddDependency(resolver.CreateDependencyOnGameEntity(gameEntityType));
            return this;
        }
    }
}
