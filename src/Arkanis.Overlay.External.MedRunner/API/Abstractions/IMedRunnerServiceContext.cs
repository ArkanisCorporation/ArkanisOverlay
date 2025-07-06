namespace Arkanis.Overlay.External.MedRunner.API.Abstractions;

using Models;

public interface IMedRunnerServiceContext : IDisposable
{
    bool IsServiceUnavailable { get; }
    bool IsServiceAvailable { get; }

    PublicOrgSettings PublicSettings { get; }
    Person? ClientInfo { get; }
    ClientBlockedStatus? ClientStatus { get; }


    IMedRunnerApiClient ApiClient { get; }

    IWebSocketEventProvider Events { get; }

    event EventHandler Updated;

    Task RefreshAsync(CancellationToken cancellationToken);
}
