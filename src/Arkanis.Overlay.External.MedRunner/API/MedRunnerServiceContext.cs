namespace Arkanis.Overlay.External.MedRunner.API;

using Abstractions;
using Common.Services;
using Domain;
using Domain.Abstractions.Services;
using Domain.Options;
using Microsoft.Extensions.Logging;
using Models;

public sealed class MedRunnerServiceContext : SelfInitializableServiceBase, IMedRunnerServiceContext
{
    private readonly IMedRunnerClientConfig _clientConfig;
    private readonly ILogger<MedRunnerServiceContext> _logger;
    private readonly IUserPreferencesManager _userPreferences;

    public MedRunnerServiceContext(
        IMedRunnerApiClient apiClient,
        IMedRunnerClientConfig clientConfig,
        IUserPreferencesManager userPreferences,
        ILogger<MedRunnerServiceContext> logger
    )
    {
        ApiClient = apiClient;
        _clientConfig = clientConfig;
        _userPreferences = userPreferences;
        _logger = logger;

        _userPreferences.UpdatePreferences += OnUpdatePreferences;
    }

    public List<string> Errors { get; set; } = [];

    public Person? ClientInfo { get; set; }

    public ClientBlockedStatus ClientStatus { get; set; } = new();

    public PublicOrgSettings PublicSettings { get; set; } = new()
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

    public IWebSocketEventProvider Events
        => ApiClient.WebSocket.Events;

    public event EventHandler? Updated;

    public IMedRunnerApiClient ApiClient { get; }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            ApiClient.OrgSettings.GetPublicSettingsAsync()
                .ContinueWith(
                    task =>
                    {
                        if (task.Result.Success)
                        {
                            PublicSettings = task.Result.Data;
                            return;
                        }

                        Errors.Add(task.Result.ErrorMessage);
                    },
                    cancellationToken
                ),
            ApiClient.Client.GetAsync()
                .ContinueWith(
                    task =>
                    {
                        if (task.Result.Success)
                        {
                            ClientInfo = task.Result.Data;
                            return;
                        }

                        Errors.Add(task.Result.ErrorMessage);
                    },
                    cancellationToken
                ),
            RefreshClientStatusAsync()
        );

        Updated?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _userPreferences.UpdatePreferences -= OnUpdatePreferences;
        ApiClient.WebSocket.Events.PersonUpdated -= OnPersonUpdated;
        ApiClient.WebSocket.Events.OrgSettingsUpdated -= OnOrgSettingsUpdated;
    }

    private async void OnUpdatePreferences(object? sender, UserPreferences currentPreferences)
    {
        var credentials = currentPreferences.GetOrCreateCredentialsFor(ExternalService.MedRunner);
        if (credentials.SecretToken is { Length: > 0 } token)
        {
            _clientConfig.SetApiToken(token);
            await RefreshAsync(CancellationToken.None);
        }
    }

    private async Task RefreshClientStatusAsync()
    {
        var response = await ApiClient.Client.GetBlockedStatusAsync();
        if (response.Success)
        {
            ClientStatus = response.Data;
            return;
        }

        Errors.Add(response.ErrorMessage);
    }

    protected override async Task InitializeAsyncCore(CancellationToken cancellationToken)
    {
        await ApiClient.WebSocket.EnsureInitializedAsync(cancellationToken);
        ApiClient.WebSocket.Events.PersonUpdated += OnPersonUpdated;
        ApiClient.WebSocket.Events.OrgSettingsUpdated += OnOrgSettingsUpdated;
        await RefreshAsync(cancellationToken);
    }

    private void OnOrgSettingsUpdated(object? _, OrgSettings currentSettings)
    {
        try
        {
            PublicSettings = currentSettings.Public;
            Updated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process settings update");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process client info update");
        }
    }
}
