namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Repositories.Exceptions;

public abstract class DependencyResolver(IServiceProvider serviceProvider)
{
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    public static Context DependsOn(object dependent, IEnumerable<IDependable> dependencies)
        => CreateDependencyContext(dependent, dependencies);

    private static Context CreateDependencyContext(object dependent, IEnumerable<IDependable> dependencies)
        => new(dependent, NullLogger.Instance, dependencies);

    protected IDependable CreateDependencyOn<T>() where T : IDependable
        => ServiceProvider.GetRequiredService<T>();

    public class Context(object dependent, ILogger logger, params IEnumerable<IDependable> dependencies) : IDependable
    {
        private readonly List<IDependable> _dependencies = dependencies.ToList();

        public object Dependent { get; } = dependent;

        protected ILogger Logger { get; } = logger;

        public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        {
            var dependencies = _dependencies
                .Select(dependency => DependentProcess.Create(dependency, cancellationToken))
                .ToList();

            DependentProcess? lastProcess = null;
            try
            {
                var monitorTask = MonitorAsync(dependencies, cancellationToken);
                var tasks = dependencies.Select(x => x.Task).Append(monitorTask).ToList();
                while (tasks.Count > 0)
                {
                    var finishedTask = await Task.WhenAny(tasks).ConfigureAwait(false);

                    lastProcess = dependencies.SingleOrDefault(process => process.Task == finishedTask);
                    if (lastProcess is not null)
                    {
                        Logger.LogDebug("Dependency {Dependency} is ready for {Dependent}", lastProcess.Dependency, Dependent);
                    }

                    await finishedTask;
                    tasks.Remove(finishedTask);

                    if (tasks.Count == 1 && tasks.Contains(monitorTask))
                    {
                        break;
                    }
                }

                Logger.LogDebug("All {DependencyCount} dependencies are ready for {Dependent}", dependencies.Count, Dependent);
            }
            catch (OperationCanceledException e)
            {
                var failedDependencies = dependencies
                    .Where(dependency => !dependency.Task.IsCompletedSuccessfully)
                    .Select(dependency => dependency.Dependency)
                    .ToArray();

                throw DependencyWaitCancelledException.Create(failedDependencies, e);
            }
            catch (Exception e)
            {
                if (lastProcess is not null)
                {
                    throw DependencyErroredException.Create(e, lastProcess.Dependency);
                }

                throw;
            }
        }

        protected virtual async Task MonitorAsync(List<DependentProcess> processes, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken).ConfigureAwait(false);
                var incompleteProcesses = processes
                    .Where(dependency => !dependency.Task.IsCompleted)
                    .Select(dependency => dependency.Dependency)
                    .ToArray();

                if (incompleteProcesses.Length <= 0)
                {
                    break;
                }

                Logger.LogWarning(
                    "Dependent {Dependent} is still waiting for {DependencyCount} dependencies to be ready: {@Dependencies}",
                    Dependent,
                    incompleteProcesses.Length,
                    incompleteProcesses
                );
            }
        }

        public Context AddDependency(IDependable dependency)
        {
            _dependencies.Add(dependency);
            return this;
        }

        public record DependentProcess(IDependable Dependency, Task Task)
        {
            public static DependentProcess Create(IDependable dependency, CancellationToken cancellationToken)
                => new(dependency, dependency.WaitUntilReadyAsync(cancellationToken));
        }
    }
}
