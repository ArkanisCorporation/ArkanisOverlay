namespace Arkanis.Overlay.External.MedRunner.API.Abstractions;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Models;

public interface IMedRunnerServiceContext : IDisposable
{
    bool IsServiceUnavailable
        => !IsServiceAvailable;

    bool IsServiceAvailable
        => PublicSettings is { EmergenciesEnabled: true, Status: not ServiceStatus.Offline and not ServiceStatus.Unknown };

    [MemberNotNullWhen(true, nameof(ClientInfo), nameof(ClientStatus), nameof(ClientIdentity))]
    public bool CanClientUseServices
        => IsClientAuthenticated
           && !IsClientInactive
           && !IsClientBlocked;

    [MemberNotNullWhen(true, nameof(ClientIdentity))]
    public bool IsClientAuthenticated
        => ClientIdentity is { IsAuthenticated: true };

    [MemberNotNullWhen(true, nameof(ClientInfo))]
    bool IsClientInactive
        => ClientInfo is { Active: false };

    [MemberNotNullWhen(true, nameof(ClientStatus))]
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
    ClaimsIdentity? ClientIdentity { get; }

    List<string> Errors { get; }

    IMedRunnerApiClient ApiClient { get; }

    IWebSocketEventProvider Events { get; }

    event EventHandler Updated;

    Task RefreshAsync(CancellationToken cancellationToken);
}
