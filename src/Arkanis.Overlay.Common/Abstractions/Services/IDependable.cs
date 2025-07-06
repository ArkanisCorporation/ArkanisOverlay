namespace Arkanis.Overlay.Common.Abstractions.Services;

/// <summary>
///     This identifies services that may need to be initialized before they can be used.
///     The interface allows the consumer to wait until the service is guaranteed to be ready.
/// </summary>
public interface IDependable
{
    bool IsReady { get; }

    Task WaitUntilReadyAsync(CancellationToken cancellationToken = default);
}
