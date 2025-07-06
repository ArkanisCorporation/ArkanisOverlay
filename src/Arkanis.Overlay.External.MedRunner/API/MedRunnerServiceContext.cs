namespace Arkanis.Overlay.External.MedRunner.API;

using Abstractions;
using Common.Services;
using Models;
using Microsoft.Extensions.Logging;

public sealed class MedRunnerServiceContext(IMedRunnerApiClient apiClient, ILogger<MedRunnerServiceContext> logger)
    : SelfInitializableServiceBase, IMedRunnerServiceContext
{
    public List<string> Errors { get; set; } = [];

    public bool HasErrors
        => Errors is { Count: > 0 };

    public Person? ClientInfo { get; set; }

    public ClientBlockedStatus ClientStatus { get; set; } = new();

    public bool IsServiceUnavailable
        => !IsServiceAvailable;

    public bool IsServiceAvailable
        => PublicSettings is { EmergenciesEnabled: true, Status: not ServiceStatus.Offline };

    public PublicOrgSettings PublicSettings { get; set; } = new()
    {
        Status = ServiceStatus.Offline,
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

    public IMedRunnerApiClient ApiClient { get; } = apiClient;

    public async Task RefreshAsync(CancellationToken cancellationToken)
        => await Task.WhenAll(
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

    public void Dispose()
    {
        ApiClient.WebSocket.Events.PersonUpdated -= OnPersonUpdated;
        ApiClient.WebSocket.Events.OrgSettingsUpdated -= OnOrgSettingsUpdated;
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
            logger.LogError(ex, "Failed to process settings update");
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
            logger.LogError(ex, "Failed to process client info update");
        }
    }
}
