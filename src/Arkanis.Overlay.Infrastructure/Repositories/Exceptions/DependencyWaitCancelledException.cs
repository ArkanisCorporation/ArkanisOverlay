namespace Arkanis.Overlay.Infrastructure.Repositories.Exceptions;

using Common.Abstractions.Services;

public class DependencyWaitCancelledException(IDependable[] dependencies, Exception? innerException = null)
    : Exception($"A dependency wait task has been cancelled for: {string.Join(',', dependencies.Select(x => x.ToString()))}", innerException)
{
    public IDependable[] Dependencies { get; } = dependencies;

    public static DependencyWaitCancelledException Create(IDependable[] dependencies, OperationCanceledException innerException)
        => new(dependencies, innerException);
}
