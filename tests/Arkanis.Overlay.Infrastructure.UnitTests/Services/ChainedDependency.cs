namespace Arkanis.Overlay.Infrastructure.UnitTests.Services;

using Domain.Abstractions;

internal class ChainedDependency(IDependable dependable) : IDependable
{
    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await dependable.WaitUntilReadyAsync(cancellationToken);

    public static ChainedDependency Create(IDependable dependable)
        => new(dependable);
}
