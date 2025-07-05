namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.WebSocket;

using Abstractions;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
///     Manages websocket connections for real-time updates.
/// </summary>
public class SignalRManager(IMedRunnerClientConfig config, IMedRunnerTokenProvider tokenProvider)
{
    public async Task<HubConnection> EstablishConnectionAsync()
    {
        var connection = new HubConnectionBuilder()
            .WithAutomaticReconnect()
            .WithUrl($"{config.BaseUrl}/hub/emergency", options => options.AccessTokenProvider = tokenProvider.GetAccessTokenAsync)
            .Build();

        await connection.StartAsync();
        return connection;
    }
}
