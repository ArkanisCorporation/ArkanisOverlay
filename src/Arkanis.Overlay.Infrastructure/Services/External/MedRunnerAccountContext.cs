namespace Arkanis.Overlay.Infrastructure.Services.External;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
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
            : Task.FromResult(Result.Fail<ClaimsIdentity>("MedRunner account linking is temporarily unavailable. Please use the MedRunner web portal."));

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
                    "MedRunner services are currently available through the web portal.");
            }

            if (!IsAuthenticated)
            {
                return new(
                    false,
                    MedRunnerServiceAccessRestriction.Account,
                    "Connect a MedRunner account before requesting service."
                );
            }

            if (IsClientBlocked)
            {
                return new(
                    false,
                    MedRunnerServiceAccessRestriction.Account,
                    "This MedRunner account is blocked and cannot request service."
                );
            }

            if (IsClientInactive)
            {
                return new(
                    false,
                    MedRunnerServiceAccessRestriction.Account,
                    "This MedRunner account does not have an active subscription and cannot request service."
                );
            }

            if (AccessStateError is { } accessStateError)
            {
                return new(
                    false,
                    MedRunnerServiceAccessRestriction.LocalProcessing,
                    $"MedRunner account or service data could not be processed: {accessStateError}"
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

            return new(true, null, "MedRunner services are available.");
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
            var publicSettingsTask = TryRequestAsync(ApiClient.OrgSettings.GetPublicSettingsAsync);
            if (!IsAuthenticated)
            {
                var errors = new List<string>();
                ApplyPublicSettings(await publicSettingsTask, errors);
                ClientInfo = null;
                ClientStatus = null;
                Errors = errors;
                Updated?.Invoke(this, EventArgs.Empty);
                return;
            }

            var clientInfoTask = TryRequestAsync(ApiClient.Client.GetAsync);
            var clientStatusTask = TryRequestAsync(ApiClient.Client.GetBlockedStatusAsync);
            await Task.WhenAll(publicSettingsTask, clientInfoTask, clientStatusTask);
            cancellationToken.ThrowIfCancellationRequested();

            var authenticatedErrors = new List<string>();
            ApplyPublicSettings(await publicSettingsTask, authenticatedErrors);
            ApplyClientInfo(await clientInfoTask, authenticatedErrors);
            ApplyClientStatus(await clientStatusTask, authenticatedErrors);
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
        await base.InitializeAsyncCore(cancellationToken);
    }

    protected override async Task OnAuthenticationStateChangedAsync(CancellationToken cancellationToken)
    {
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
            Logger.LogWarning(exception, "Failed to establish MedRunner real-time updates");
            Errors = [.. Errors, $"Unable to establish MedRunner real-time updates: {exception.Message}"];
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
            ? $"Unable to retrieve MedRunner {operation} (status {(int)response.StatusCode}: {response.StatusCode})."
            : $"Unable to retrieve MedRunner {operation}: {response.ErrorMessage}";

    private string DescribeUnavailableAccountData()
        => AccessStateError is { } accessStateError
            ? $"MedRunner account data could not be processed: {accessStateError}"
            : "MedRunner account data is unavailable. Refresh the connection or reconnect your account.";

    private string DescribeUnavailableService()
        => PublicSettings switch
            {
                { EmergenciesEnabled: false } => "MedRunner has temporarily disabled new emergency requests for this organization.",
                { Status: ServiceStatus.Offline } => "MedRunner is currently offline and cannot accept emergency requests.",
                _ => "MedRunner service availability is unknown and cannot accept emergency requests.",
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
            Logger.LogError(exception, "Failed to process MedRunner settings update");
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
            Logger.LogError(exception, "Failed to process MedRunner client update");
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
