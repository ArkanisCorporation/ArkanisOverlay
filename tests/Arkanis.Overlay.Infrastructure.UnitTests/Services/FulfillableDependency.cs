namespace Arkanis.Overlay.Infrastructure.UnitTests.Services;

using System.Threading;
using System.Threading.Tasks;
using Common.Abstractions.Services;
using Domain.Abstractions;

internal sealed class FulfillableDependency : IDependable
{
    private readonly TaskCompletionSource _completionSource = new();

    public bool IsReady
        => _completionSource.Task.IsCompletedSuccessfully;

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => _completionSource.Task;

    public void Fulfill()
        => _completionSource.SetResult();

    public static FulfillableDependency Create()
        => new();
}
