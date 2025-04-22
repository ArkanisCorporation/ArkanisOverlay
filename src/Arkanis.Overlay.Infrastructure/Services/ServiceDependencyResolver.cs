namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions;
using Microsoft.Extensions.Logging;

public class ServiceDependencyResolver(IServiceProvider serviceProvider, ILogger<ServiceDependencyResolver> logger) : DependencyResolver(serviceProvider)
{
    private readonly ILogger<ServiceDependencyResolver> _logger = logger;

    public Context DependsOn<T>(object dependent) where T : IDependable
        => CreateDependencyContextOn<T>(dependent);

    private Context CreateDependencyContextOn<T>(object dependent) where T : IDependable
        => CreateDependencyContext(dependent, CreateDependencyOn<T>());

    private Context CreateDependencyContext(object dependent, params IEnumerable<IDependable> dependencies)
        => new(dependent, this, dependencies);

    public new sealed class Context(object dependent, ServiceDependencyResolver resolver, params IEnumerable<IDependable> dependencies)
        : DependencyResolver.Context(dependent, resolver._logger, dependencies)
    {
        public Context AlsoDependsOn<T>() where T : IDependable
        {
            AddDependency(resolver.CreateDependencyOn<T>());
            return this;
        }
    }
}
