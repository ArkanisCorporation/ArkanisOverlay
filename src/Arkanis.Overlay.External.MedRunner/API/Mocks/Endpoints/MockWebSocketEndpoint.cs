namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using Abstractions;
using Abstractions.Endpoints;

public class MockWebSocketEndpoint(IWebSocketEventProvider eventProvider, IMedRunnerTokenProvider tokenProvider)
    : MockApiEndpoint(tokenProvider), IWebSocketEndpoint
{
    public IWebSocketEventProvider Events { get; } = eventProvider;

    public Task EnsureInitializedAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task DisconnectAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
