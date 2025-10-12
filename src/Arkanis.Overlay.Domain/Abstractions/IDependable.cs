namespace Arkanis.Overlay.Domain.Abstractions;

/// <summary>
///     This identifies services that may need to be initialized before they can be used.
///     The interface allows the consumer to wait until the service is guaranteed to be ready.
/// </summary>
public interface IDependable
{
    public bool IsReady { get; }

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default);
}
