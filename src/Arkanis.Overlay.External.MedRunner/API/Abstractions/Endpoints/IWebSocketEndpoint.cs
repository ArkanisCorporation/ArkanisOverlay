namespace Arkanis.Overlay.External.MedRunner.API.Abstractions.Endpoints;

/// <summary>
///     Endpoints for interacting with websocket/realtime updates.
/// </summary>
public interface IWebSocketEndpoint
{
    /// <summary>
    ///     Gets the WebSocket event provider for subscribing to real-time events.
    /// </summary>
    IWebSocketEventProvider Events { get; }

    /// <summary>
    ///     Ensures the WebSocket connection is initialized and connected.
    ///     If not already connected, establishes a new WebSocket connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnsureInitializedAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Stops the active WebSocket connection, if any.
    ///     Use this before switching the authenticated account so subscriptions cannot continue under previous credentials.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DisconnectAsync(CancellationToken cancellationToken);
}
