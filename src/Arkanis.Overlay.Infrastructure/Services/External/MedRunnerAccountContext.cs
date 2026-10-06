namespace Arkanis.Overlay.Infrastructure.Services.External;

using global::Arkanis.Overlay.Common.Abstractions;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using global::Arkanis.Overlay.Common.Models;
using global::Arkanis.Overlay.Common.Options;
using global::Arkanis.Overlay.Common.Services;
using global::Arkanis.Overlay.Domain.Abstractions.Services;
using global::Arkanis.Overlay.External.MedRunner;
using global::Arkanis.Overlay.External.MedRunner.API;
using global::Arkanis.Overlay.External.MedRunner.API.Abstractions;
using global::Arkanis.Overlay.External.MedRunner.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class MedRunnerAccountContext(
    MedRunnerAuthenticator authenticator,
    IMedRunnerTokenProvider tokenProvider,
    IMedRunnerApiClient apiClient,
    IUserPreferencesManager userPreferences,
    ILogger<MedRunnerAccountContext> logger,
    IOptions<MedRunnerIntegrationOptions>? integrationOptions = null
) : ExternalAccountContext<MedRunnerAuthenticator.AuthenticationTask>(authenticator, userPreferences, logger)
{
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);
    private readonly object _emergencyLock = new();
    private readonly Dictionary<string, long> _emergencyVersions = new(StringComparer.Ordinal);
    private long _emergencyVersion;
    private string? _credentialFingerprint;
    private string? _principalName;
    private string? _clientInfoError;
    private string? _clientStatusError;
    private string? _publicSettingsError;

    public bool AccountLinkingEnabled { get; } = integrationOptions?.Value.AccountLinkingEnabled ?? false;

    public override bool IsAuthenticated
        => AccountLinkingEnabled && base.IsAuthenticated;

    public override Task UpdateAsync(CancellationToken cancellationToken)
    {
        if (AccountLinkingEnabled)
        {
            return base.UpdateAsync(cancellationToken);
        }

        tokenProvider.ClearAuthentication();
        return Task.CompletedTask;
    }

    public override Task<Result<ClaimsIdentity>> ConfigureAsync(AccountCredentials credentials, CancellationToken cancellationToken)
        => AccountLinkingEnabled
            ? base.ConfigureAsync(credentials, cancellationToken)
            : Task.FromResult(Result.Fail<ClaimsIdentity>("Medrunner account linking is temporarily unavailable. Please use the Medrunner web portal."));

    public IMedRunnerApiClient ApiClient { get; } = apiClient;

    public IWebSocketEventProvider Events
        => ApiClient.WebSocket.Events;

    public PublicOrgSettings PublicSettings { get; private set; } = new()
    {
        Status = ServiceStatus.Unknown,
        EmergenciesEnabled = false,
        AnonymousAlertsEnabled = false,
        RegistrationEnabled = false,
        MessageOfTheDay = null,
        LocationSettings = new LocationSettings
        {
            Locations = [],
        },
    };

    public Person? ClientInfo { get; private set; }

    public IReadOnlyList<Emergency> Emergencies { get; private set; } = [];

    public string? EmergencyLoadError { get; private set; }

    public long AccountGeneration { get; private set; }

    private string? _emergencyClientId;

    public ClientBlockedStatus? ClientStatus { get; private set; }

    public List<string> Errors { get; private set; } = [];

    public bool HasErrors
        => Errors.Count > 0;

    public bool IsServiceUnavailable
        => !IsServiceAvailable;

    public bool IsServiceAvailable
        => PublicSettings is { EmergenciesEnabled: true, Status: not ServiceStatus.Offline and not ServiceStatus.Unknown };

    private string? AccessStateError
        => _publicSettingsError ?? _clientInfoError ?? _clientStatusError;

    public MedRunnerServiceAccessState ServiceAccessState
    {
        get
        {
            if (!AccountLinkingEnabled)
            {
                return new(false, MedRunnerServiceAccessRestriction.Account,
                    "Medrunner services are currently available through the web portal.");
            }

            if (!IsAuthenticated)
            {
                return new(
                    false,
                    MedRunnerServiceAccessRestriction.Account,
                    "Connect a Medrunner account before requesting service."
                );
            }

            if (IsClientBlocked)
            {
                return new(
                    false,
                    MedRunnerServiceAccessRestriction.Account,
                    "This Medrunner account is blocked and cannot request service."
                );
            }

            if (IsClientInactive)
            {
                return new(
                    false,
                    MedRunnerServiceAccessRestriction.Account,
                    "This Medrunner account does not have an active subscription and cannot request service."
                );
            }

            if (AccessStateError is { } accessStateError)
            {
                return new(
                    false,
                    MedRunnerServiceAccessRestriction.LocalProcessing,
                    $"Medrunner account or service data could not be processed: {accessStateError}"
                );
            }

            if (ClientInfo is null || ClientStatus is null)
            {
                return new(
                    false,
                    MedRunnerServiceAccessRestriction.LocalProcessing,
                    DescribeUnavailableAccountData()
                );
            }

            if (!IsServiceAvailable)
            {
                return new(
                    false,
                    MedRunnerServiceAccessRestriction.Organization,
                    DescribeUnavailableService()
                );
            }

            return new(true, null, "Medrunner services are available.");
        }
    }

    [MemberNotNullWhen(true, nameof(ClientInfo), nameof(ClientStatus))]
    public bool CanClientUseServices
        => IsAuthenticated && ClientInfo is not null && ClientStatus is not null && !IsClientInactive && !IsClientBlocked;

    [MemberNotNullWhen(true, nameof(ClientInfo))]
    public bool IsClientInactive
        => ClientInfo is { Active: false };

    [MemberNotNullWhen(true, nameof(ClientStatus))]
    public bool IsClientBlocked
        => ClientStatus is { Blocked: true };

    public bool IsDisabled
        => !IsEnabled;

    public bool IsEnabled
        => ServiceAccessState.CanUseServices;

    public event EventHandler? Updated;

    public override void Dispose()
    {
        ApiClient.WebSocket.Events.PersonUpdated -= OnPersonUpdated;
        ApiClient.WebSocket.Events.OrgSettingsUpdated -= OnOrgSettingsUpdated;
        ApiClient.WebSocket.Events.EmergencyCreated -= OnEmergencyChanged;
        ApiClient.WebSocket.Events.EmergencyUpdated -= OnEmergencyChanged;
        _refreshSemaphore.Dispose();
        base.Dispose();
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        if (!AccountLinkingEnabled)
        {
            return;
        }

        await _refreshSemaphore.WaitAsync(cancellationToken);
        try
        {
            var accountGeneration = AccountGeneration;
            var publicSettingsTask = TryRequestAsync(ApiClient.OrgSettings.GetPublicSettingsAsync);
            if (!IsAuthenticated)
            {
                var errors = new List<string>();
                var publicSettings = await publicSettingsTask;
                lock (_emergencyLock)
                {
                    if (accountGeneration != AccountGeneration)
                    {
                        return;
                    }

                    ApplyPublicSettings(publicSettings, errors);
                    ClientInfo = null;
                    ClientStatus = null;
                    ClearEmergencies();
                    EmergencyLoadError = null;
                    _emergencyClientId = null;
                    Errors = errors;
                }

                Updated?.Invoke(this, EventArgs.Empty);
                return;
            }

            var clientInfoTask = TryRequestAsync(ApiClient.Client.GetAsync);
            var clientStatusTask = TryRequestAsync(ApiClient.Client.GetBlockedStatusAsync);
            await Task.WhenAll(publicSettingsTask, clientInfoTask, clientStatusTask);
            cancellationToken.ThrowIfCancellationRequested();
            var authenticatedErrors = new List<string>();
            var publicSettingsResponse = await publicSettingsTask;
            var clientInfoResponse = await clientInfoTask;
            var clientStatusResponse = await clientStatusTask;
            lock (_emergencyLock)
            {
                if (accountGeneration != AccountGeneration)
                {
                    return;
                }

                ApplyPublicSettings(publicSettingsResponse, authenticatedErrors);
                ApplyClientInfo(clientInfoResponse, authenticatedErrors);
                ApplyClientStatus(clientStatusResponse, authenticatedErrors);
            }
            await LoadEmergenciesAsync(accountGeneration, cancellationToken);
            if (accountGeneration != AccountGeneration)
            {
                return;
            }
            Errors = authenticatedErrors;
            Updated?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    protected override async Task InitializeAsyncCore(CancellationToken cancellationToken)
    {
        if (!AccountLinkingEnabled)
        {
            tokenProvider.ClearAuthentication();
            return;
        }

        ApiClient.WebSocket.Events.PersonUpdated += OnPersonUpdated;
        ApiClient.WebSocket.Events.OrgSettingsUpdated += OnOrgSettingsUpdated;
        ApiClient.WebSocket.Events.EmergencyCreated += OnEmergencyChanged;
        ApiClient.WebSocket.Events.EmergencyUpdated += OnEmergencyChanged;
        await base.InitializeAsyncCore(cancellationToken);
    }

    protected override async Task OnAuthenticationStateChangedAsync(CancellationToken cancellationToken)
    {
        var credentials = CurrentAuthentication?.Credentials as AccountApiTokenCredentials;
        var fingerprint = IsAuthenticated && credentials is not null
            ? Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credentials.SecretToken)))
            : null;
        var principalName = IsAuthenticated ? Identity.Name : null;
        var accountChanged = false;
        lock (_emergencyLock)
        {
            if (_credentialFingerprint != fingerprint || _principalName != principalName)
            {
                _credentialFingerprint = fingerprint;
                _principalName = principalName;
                AccountGeneration++;
                ClientInfo = null;
                ClientStatus = null;
                ClearEmergencies();
                _emergencyClientId = null;
                EmergencyLoadError = null;
                accountChanged = true;
            }
        }

        if (accountChanged)
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }

        if (CurrentAuthentication?.Authentication is { } authentication)
        {
            tokenProvider.ApplyAuthentication(authentication);
        }
        else
        {
            tokenProvider.ClearAuthentication();
        }

        await ApiClient.WebSocket.DisconnectAsync(cancellationToken);
        await RefreshAsync(cancellationToken);
        if (!IsAuthenticated)
        {
            return;
        }

        try
        {
            await ApiClient.WebSocket.EnsureInitializedAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Failed to establish Medrunner real-time updates");
            Errors = [.. Errors, $"Unable to establish Medrunner real-time updates: {exception.Message}"];
            Updated?.Invoke(this, EventArgs.Empty);
        }
    }

    private async Task RefreshClientStatusAsync()
    {
        await _refreshSemaphore.WaitAsync();
        try
        {
            var response = await TryRequestAsync(ApiClient.Client.GetBlockedStatusAsync);
            var errors = new List<string>();
            ApplyClientStatus(response, errors);
            if (errors.Count > 0)
            {
                Errors = [.. Errors, .. errors];
            }

            Updated?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    private async Task LoadEmergenciesAsync(long accountGeneration, CancellationToken cancellationToken)
    {
        if (ClientInfo is not { Id: { } clientId })
        {
            EmergencyLoadError = _clientInfoError ?? "Unable to load Medrunner alerts without an account profile.";
            return;
        }

        if (_emergencyClientId != clientId)
        {
            ClearEmergencies();
            _emergencyClientId = clientId;
        }

        EmergencyLoadError = null;
        var emergencyIds = new HashSet<string>(StringComparer.Ordinal);
        var paginationTokens = new HashSet<string>(StringComparer.Ordinal);
        string? paginationToken = null;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await TryRequestAsync(() => ApiClient.Client.GetHistoryAsync(50, paginationToken));
            cancellationToken.ThrowIfCancellationRequested();
            if (accountGeneration != AccountGeneration)
            {
                return;
            }
            if (response is not { Success: true, Data: { } page })
            {
                EmergencyLoadError = DescribeFailure("emergency history", response);
                return;
            }

            foreach (var history in page.Data.Where(history => history.ClientId == clientId))
            {
                emergencyIds.Add(history.EmergencyId);
            }

            paginationToken = page.PaginationToken;
        } while (!string.IsNullOrEmpty(paginationToken) && paginationTokens.Add(paginationToken));

        foreach (var batch in emergencyIds.Chunk(50))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, long> observedVersions;
            lock (_emergencyLock)
            {
                observedVersions = new Dictionary<string, long>(_emergencyVersions, StringComparer.Ordinal);
            }

            var response = await TryRequestAsync(() => ApiClient.Emergency.GetEmergenciesAsync([.. batch]));
            cancellationToken.ThrowIfCancellationRequested();
            if (accountGeneration != AccountGeneration)
            {
                return;
            }
            if (response is not { Success: true, Data: { } emergencies })
            {
                EmergencyLoadError = DescribeFailure("emergency details", response);
                return;
            }

            // The IDs came from this client's history; never accept an unrelated bulk response.
            foreach (var emergency in emergencies.Where(emergency => batch.Contains(emergency.Id)
                         && (emergency.ClientId is null || emergency.ClientId == clientId)))
            {
                lock (_emergencyLock)
                {
                    // A newer live event must win over a bulk response that was already in flight.
                    if (accountGeneration == AccountGeneration
                        && _emergencyVersions.GetValueOrDefault(emergency.Id) == observedVersions.GetValueOrDefault(emergency.Id))
                    {
                        StoreEmergency(emergency);
                    }
                }
            }
        }
    }

    public void RecordEmergency(Emergency emergency)
    {
        lock (_emergencyLock)
        {
            if (!IsAuthenticated || ClientInfo is null || emergency.ClientId != ClientInfo.Id)
            {
                return;
            }

            StoreEmergency(emergency);
        }

        Updated?.Invoke(this, EventArgs.Empty);
    }

    private void StoreEmergency(Emergency emergency)
    {
        lock (_emergencyLock)
        {
            Emergencies = Emergencies.Where(existing => existing.Id != emergency.Id)
                .Append(emergency)
                .OrderByDescending(existing => existing.Status is MissionStatus.Pending or MissionStatus.Accepted)
                .ThenByDescending(existing => existing.CreationTimestamp)
                .ToArray();
            _emergencyVersions[emergency.Id] = ++_emergencyVersion;
        }
    }

    private void ClearEmergencies()
    {
        lock (_emergencyLock)
        {
            Emergencies = [];
            _emergencyVersions.Clear();
        }
    }

    private void OnEmergencyChanged(object? _, Emergency emergency)
        => RecordEmergency(emergency);

    private static async Task<ApiResponse<T>> TryRequestAsync<T>(Func<Task<ApiResponse<T>>> request)
    {
        try
        {
            return await request();
        }
        catch (Exception exception)
        {
            return new ApiResponse<T>(exception);
        }
    }

    private void ApplyPublicSettings(ApiResponse<PublicOrgSettings> response, List<string> errors)
    {
        if (response is { Success: true, Data: { } settings })
        {
            PublicSettings = settings;
            _publicSettingsError = null;
            return;
        }

        _publicSettingsError = DescribeFailure("public settings", response);
        errors.Add(_publicSettingsError);
    }

    private void ApplyClientInfo(ApiResponse<Person> response, List<string> errors)
    {
        if (response is { Success: true, Data: { } clientInfo })
        {
            ClientInfo = clientInfo;
            _clientInfoError = null;
            return;
        }

        _clientInfoError = DescribeFailure("account profile", response);
        errors.Add(_clientInfoError);
    }

    private void ApplyClientStatus(ApiResponse<ClientBlockedStatus> response, List<string> errors)
    {
        if (response is { Success: true, Data: { } clientStatus })
        {
            ClientStatus = clientStatus;
            _clientStatusError = null;
            return;
        }

        _clientStatusError = DescribeFailure("account status", response);
        errors.Add(_clientStatusError);
    }

    private static string DescribeFailure<T>(string operation, ApiResponse<T> response)
        => string.IsNullOrWhiteSpace(response.ErrorMessage)
            ? $"Unable to retrieve Medrunner {operation} (status {(int)response.StatusCode}: {response.StatusCode})."
            : $"Unable to retrieve Medrunner {operation}: {response.ErrorMessage}";

    private string DescribeUnavailableAccountData()
        => AccessStateError is { } accessStateError
            ? $"Medrunner account data could not be processed: {accessStateError}"
            : "Medrunner account data is unavailable. Refresh the connection or reconnect your account.";

    private string DescribeUnavailableService()
        => PublicSettings switch
            {
                { EmergenciesEnabled: false } => "Medrunner has temporarily disabled new emergency requests for this organization.",
                { Status: ServiceStatus.Offline } => "Medrunner is currently offline and cannot accept emergency requests.",
                _ => "Medrunner service availability is unknown and cannot accept emergency requests.",
            };

    private void OnOrgSettingsUpdated(object? _, OrgSettings currentSettings)
    {
        try
        {
            PublicSettings = currentSettings.Public;
            Updated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to process Medrunner settings update");
        }
    }

    private async void OnPersonUpdated(object? _, Person currentClientInfo)
    {
        try
        {
            ClientInfo = currentClientInfo;
            Updated?.Invoke(this, EventArgs.Empty);
            await RefreshClientStatusAsync();
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to process Medrunner client update");
        }
    }
}

public enum MedRunnerServiceAccessRestriction
{
    Account,
    Organization,
    LocalProcessing,
}

public sealed record MedRunnerServiceAccessState(
    bool CanUseServices,
    MedRunnerServiceAccessRestriction? Restriction,
    string Message
);
