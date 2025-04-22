namespace Arkanis.Overlay.Infrastructure.Repositories.Exceptions;

using Domain.Abstractions;

public class DependencyErroredException(IDependable dependency, Exception? innerException = null)
    : Exception($"Dependency has failed to initialize: {dependency}", innerException)
{
    public IDependable Dependency { get; } = dependency;

    public static DependencyErroredException Create(Exception innerException, IDependable dependency)
        => new(dependency, innerException);
}
