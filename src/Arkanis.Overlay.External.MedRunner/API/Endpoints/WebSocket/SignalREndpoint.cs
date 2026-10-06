namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.WebSocket;

using Abstractions;
using Abstractions.Endpoints;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

/// <inheritdoc cref="IWebSocketEndpoint" />
public class SignalREndpoint(IMedRunnerClientConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger<SignalREndpoint> logger)
    : ApiEndpoint(config, tokenProvider, cache, logger), IWebSocketEndpoint, IAsyncDisposable
{
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    private readonly SignalRManager _manager = new(config, tokenProvider);
    private readonly SignalRMessageHandler _messageHandler = new(logger);
    private HubConnection? _connection;

    /// <inheritdoc />
    protected override string Endpoint
        => "websocket";

    /// <inheritdoc />
    public IWebSocketEventProvider Events
        => _messageHandler;

    /// <inheritdoc />
    public async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_messageHandler.IsConnected)
            {
                return;
            }

            _connection = await _manager.EstablishConnectionAsync(cancellationToken);
            _messageHandler.Connect(_connection);
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_connection is not { } connection)
            {
                return;
            }

            _connection = null;
            _messageHandler.Disconnect(connection);
            await connection.DisposeAsync();
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connectionSemaphore.WaitAsync();
        try
        {
            if (_connection is not { } connection)
            {
                return;
            }

            _connection = null;
            _messageHandler.Disconnect(connection);
            await connection.DisposeAsync();
        }
        finally
        {
            _connectionSemaphore.Release();
            _connectionSemaphore.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
