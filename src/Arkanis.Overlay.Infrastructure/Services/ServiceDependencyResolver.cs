namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions;
using Repositories.Exceptions;

internal abstract class ServiceDependencyResolver(IServiceProvider serviceProvider)
{
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    public class Context(ServiceDependencyResolver resolver, params IEnumerable<IDependable> dependencies) : IDependable
    {
        protected readonly List<IDependable> Dependencies = dependencies.ToList();

        public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        {
            var dependencies = Dependencies
                .Select(dependency => (Instance: dependency, Task: dependency.WaitUntilReadyAsync(cancellationToken)))
                .ToList();

            try
            {
                var tasks = dependencies.Select(x => x.Task).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {
                var failedDependencies = dependencies
                    .Where(dependency => !dependency.Task.IsCompletedSuccessfully)
                    .Select(dependency => dependency.Instance)
                    .ToArray();

                throw DependencyWaitCancelledException.Create(failedDependencies, e);
            }
        }
    }
}
