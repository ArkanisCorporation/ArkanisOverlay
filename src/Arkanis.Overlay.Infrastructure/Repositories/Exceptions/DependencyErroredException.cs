namespace Arkanis.Overlay.Infrastructure.Repositories.Exceptions;

using Common.Abstractions.Services;

public class DependencyErroredException(IDependable dependency, Exception? innerException = null)
    : Exception($"A dependency has failed to initialize: {dependency}", innerException)
{
    public IDependable Dependency { get; } = dependency;

    public static DependencyErroredException Create(Exception innerException, IDependable dependency)
        => new(dependency, innerException);
}
