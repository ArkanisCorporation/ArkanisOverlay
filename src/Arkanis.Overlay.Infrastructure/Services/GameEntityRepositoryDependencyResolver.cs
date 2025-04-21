namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Provides support logic for resolving multiple dependencies at once.
/// </summary>
/// <param name="serviceProvider">Service provider used for typed service resolution</param>
internal sealed class GameEntityRepositoryDependencyResolver(IServiceProvider serviceProvider) : ServiceDependencyResolver(serviceProvider)
{
    public Context DependsOn(IEnumerable<IDependable> dependencies)
        => CreateDependencyContext(dependencies);

    public Context DependsOn<T>() where T : class, IGameEntity
        => CreateDependencyContextOn<T>();

    public Context DependsOn(Type gameEntityType)
        => CreateDependencyContextOn(gameEntityType);

    private Context CreateDependencyContextOn<T>() where T : class, IGameEntity
        => CreateDependencyContext(CreateDependencyOn<T>());

    private Context CreateDependencyContextOn(Type gameEntityType)
        => CreateDependencyContext(CreateDependencyOn(gameEntityType));

    private IDependable CreateDependencyOn<T>() where T : class, IGameEntity
        => ServiceProvider.GetRequiredService<IGameEntityRepository<T>>();

    private IDependable CreateDependencyOn(Type gameEntityType)
    {
        var repositoryType = typeof(IGameEntityRepository<>).MakeGenericType(gameEntityType);
        var service = ServiceProvider.GetRequiredService(repositoryType);
        var repository = service as IDependable ?? throw new InvalidOperationException($"failed to resolve {repositoryType} for {gameEntityType}.");
        return repository;
    }

    private Context CreateDependencyContext(params IEnumerable<IDependable> dependencies)
        => new(this, dependencies);

    public new sealed class Context(GameEntityRepositoryDependencyResolver resolver, params IEnumerable<IDependable> dependencies)
        : ServiceDependencyResolver.Context(resolver, dependencies)
    {
        public Context AlsoDependsOn<T>() where T : class, IGameEntity
        {
            Dependencies.Add(resolver.CreateDependencyOn<T>());
            return this;
        }

        public Context AlsoDependsOn(Type gameEntityType)
        {
            Dependencies.Add(resolver.CreateDependencyOn(gameEntityType));
            return this;
        }
    }
}
