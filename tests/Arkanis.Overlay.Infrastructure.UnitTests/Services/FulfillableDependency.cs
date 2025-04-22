namespace Arkanis.Overlay.Infrastructure.UnitTests.Services;

using Domain.Abstractions;

internal class FulfillableDependency : IDependable
{
    private readonly TaskCompletionSource _completionSource = new();

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => _completionSource.Task;

    public void Fulfill()
        => _completionSource.SetResult();

    public static FulfillableDependency Create()
        => new();
}
