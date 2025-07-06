#pragma warning disable CA1716
namespace Arkanis.Overlay.Components.Shared;
#pragma warning restore CA1716
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using External.MedRunner.API.Abstractions;
using External.MedRunner.Models;
using Microsoft.AspNetCore.Components;

public abstract class MedRunnerComponentBase : ComponentBase
{
    protected string? EmergencyId
        => Emergency?.Id;

    public Emergency? Emergency
        => Context.Emergency;

    [MemberNotNullWhen(true, nameof(Emergency), nameof(EmergencyId))]
    public bool HasEmergency
        => Emergency is not null;

    [MemberNotNullWhen(true, nameof(Emergency), nameof(EmergencyId))]
    public bool HasEmergencyInProgress
        => Emergency is { Status: MissionStatus.Pending or MissionStatus.Accepted };

    [Inject]
    public required IMedRunnerApiClient MedRunner { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public EventCallback<bool> IsLoadingChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public required ContextModel Context { get; set; }

    [Parameter]
    public EventCallback<ContextModel> ContextChanged { get; set; }

    [Parameter]
    public string? ContentId { get; set; }

    protected async Task PerformSafeAsync(Func<Task> asyncAction)
    {
        try
        {
            await IsLoadingAsync();
            await asyncAction();
        }
        finally
        {
            await IsLoadingAsync(false);
        }
    }

    private async Task IsLoadingAsync(bool isLoading = true)
    {
        IsLoading = isLoading;
        await IsLoadingChanged.InvokeAsync(IsLoading);
    }

    public sealed class ContextModel : IDisposable
    {
        private IMedRunnerApiClient? _api;

        public bool IsDisabled
            => IsEnabled is false;

        public bool IsEnabled
            => ClientHasValidSubscription && IsServiceAvailable;

        public bool IsServiceUnavailable
            => !IsServiceAvailable;

        public bool IsServiceAvailable
            => Settings is { EmergenciesEnabled: true };

        public bool IsClientAuthenticated
            => _api?.TokenProvider.IsAuthenticated ?? false;

        [MemberNotNullWhen(true, nameof(ClientInfo))]
        public bool ClientHasValidSubscription
            => ClientInfo is { Active: true }
               && ClientStatus is { Blocked: false };

        public bool ClientIsInactive
            => ClientInfo is { Active: false };

        public bool ClientIsBlocked
            => ClientStatus is { Blocked: true };

        [MemberNotNullWhen(true, nameof(Emergency))]
        public bool IsEmergencyInProgress
            => Emergency is { CompletionTimestamp: null };

        public bool HasErrors
            => Errors is { Count: > 0 };

        public List<string> Errors { get; init; } = [];

        public Emergency? Emergency { get; set; }

        public Person? ClientInfo { get; set; }
        public ClientBlockedStatus ClientStatus { get; set; } = new();

        public ClaimsIdentity ClientIdentity { get; set; } = new();

        public PublicOrgSettings Settings { get; set; } = new()
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

        public void Dispose()
        {
            if (_api is null)
            {
                return;
            }

            _api.WebSocket.Events.EmergencyUpdated -= OnEmergencyUpdated;
        }

        public event EventHandler? Updated;

        public async Task UpdateAsync(IMedRunnerApiClient api)
        {
            Errors.Clear();

            await EnsureInitializedAsync(api);

            var settingsResponse = await _api.OrgSettings.GetPublicSettingsAsync();
            if (settingsResponse.Success)
            {
                Settings = settingsResponse.Data;
                if (_api.TokenProvider.IsAuthenticated)
                {
                    ClientIdentity = _api.TokenProvider.Identity;
                }
            }
            else
            {
                Errors.Add(settingsResponse.ErrorMessage);
                return;
            }

            await UpdateClientAsync(_api);
        }

        private async Task UpdateClientAsync(IMedRunnerApiClient api)
        {
            Errors.Clear();

            var clientBlockedResponse = await api.Client.GetBlockedStatusAsync();
            if (clientBlockedResponse.Success)
            {
                ClientStatus = clientBlockedResponse.Data;
            }
            else
            {
                Errors.Add(clientBlockedResponse.ErrorMessage);
                return;
            }

            var clientResponse = await api.Client.GetAsync();
            if (clientResponse.Success)
            {
                ClientInfo = clientResponse.Data;
            }
            else
            {
                Errors.Add(clientResponse.ErrorMessage);
            }
        }

        [MemberNotNull(nameof(_api))]
        private async Task EnsureInitializedAsync(IMedRunnerApiClient api)
        {
            if (_api is not null)
            {
                return;
            }

            _api = api;
            await _api.WebSocket.EnsureInitializedAsync();
            _api.WebSocket.Events.EmergencyUpdated += OnEmergencyUpdated;
        }

        private void OnEmergencyUpdated(object? _, Emergency emergency)
        {
            if (emergency.Id != Emergency?.Id)
            {
                return;
            }

            Emergency = emergency;
            Updated?.Invoke(this, EventArgs.Empty);
        }
    }
}
