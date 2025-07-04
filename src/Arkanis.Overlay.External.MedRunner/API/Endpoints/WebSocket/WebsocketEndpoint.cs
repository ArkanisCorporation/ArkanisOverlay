namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.WebSocket;

using Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

/// <summary>
///     Endpoints for interacting with websocket/realtime updates.
/// </summary>
public class WebsocketEndpoint(ApiConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger)
{
    private readonly WebsocketManager _websocketManager = new(config, tokenProvider);

    public WebsocketManager.MessageHandler Events { get; } = new();

    protected override string Endpoint
        => "websocket";

    /// <summary>
    ///     Gets realtime updates (establishes websocket connection).
    /// </summary>
    public async Task EnsureInitializedAsync()
    {
        if (Events.IsConnected)
        {
            return;
        }

        var connection = await _websocketManager.EstablishConnectionAsync();
        Events.Connect(connection);
    }
}
