namespace Arkanis.Overlay.Domain.Models;

using Abstractions;

public sealed record NoDependency : IDependable
{
    public static readonly IDependable Instance = new NoDependency();

    public bool IsReady
        => true;

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
