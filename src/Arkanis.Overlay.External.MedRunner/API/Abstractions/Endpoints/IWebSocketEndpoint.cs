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
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnsureInitializedAsync();
}
