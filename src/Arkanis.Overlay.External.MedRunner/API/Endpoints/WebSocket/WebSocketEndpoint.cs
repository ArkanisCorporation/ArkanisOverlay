namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.WebSocket;

using Abstractions;
using Abstractions.Endpoints;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

/// <inheritdoc cref="IWebsocketEndpoint" />
public class WebSocketEndpoint(ApiConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger), IWebsocketEndpoint
{
    private readonly SignalRManager _manager = new(config, tokenProvider);
    private readonly SignalRMessageHandler _messageHandler = new();

    /// <inheritdoc />
    protected override string Endpoint
        => "websocket";

    /// <inheritdoc />
    public IWebSocketEventProvider Events
        => _messageHandler;

    /// <inheritdoc />
    public async Task EnsureInitializedAsync()
    {
        if (_messageHandler.IsConnected)
        {
            return;
        }

        var connection = await _manager.EstablishConnectionAsync();
        _messageHandler.Connect(connection);
    }
}
