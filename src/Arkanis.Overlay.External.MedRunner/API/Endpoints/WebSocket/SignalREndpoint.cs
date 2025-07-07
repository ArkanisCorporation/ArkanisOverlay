namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.WebSocket;

using Abstractions;
using Abstractions.Endpoints;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

/// <inheritdoc cref="IWebSocketEndpoint" />
public class SignalREndpoint(IMedRunnerClientConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger<SignalREndpoint> logger)
    : ApiEndpoint(config, tokenProvider, cache, logger), IWebSocketEndpoint
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
    public async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_messageHandler.IsConnected)
        {
            return;
        }

        var connection = await _manager.EstablishConnectionAsync(cancellationToken);
        _messageHandler.Connect(connection);
    }
}
