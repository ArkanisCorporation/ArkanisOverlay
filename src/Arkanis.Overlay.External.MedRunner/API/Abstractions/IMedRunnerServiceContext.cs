namespace Arkanis.Overlay.External.MedRunner.API.Abstractions;

using Models;

public interface IMedRunnerServiceContext : IDisposable
{
    bool IsServiceUnavailable
        => !IsServiceAvailable;

    bool IsServiceAvailable
        => PublicSettings is { EmergenciesEnabled: true, Status: not ServiceStatus.Offline and not ServiceStatus.Unknown };

    public bool CanClientUseServices
        => IsClientAuthenticated
           && !IsClientInactive
           && !IsClientBlocked;

    public bool IsClientAuthenticated
        => ApiClient.TokenProvider.IsAuthenticated;

    bool IsClientInactive
        => ClientInfo is { Active: false };

    bool IsClientBlocked
        => ClientStatus is { Blocked: true };

    public bool IsDisabled
        => IsEnabled is false;

    public bool IsEnabled
        => CanClientUseServices
           && IsServiceAvailable;

    bool HasErrors
        => Errors is { Count: > 0 };

    PublicOrgSettings PublicSettings { get; }
    Person? ClientInfo { get; }
    ClientBlockedStatus? ClientStatus { get; }

    List<string> Errors { get; }

    IMedRunnerApiClient ApiClient { get; }

    IWebSocketEventProvider Events { get; }

    event EventHandler Updated;

    Task RefreshAsync(CancellationToken cancellationToken);
}
