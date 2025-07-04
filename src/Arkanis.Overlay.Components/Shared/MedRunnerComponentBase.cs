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
    protected string EmergencyId
        => Context.Emergency?.ClientId ?? string.Empty;

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

    public class ContextModel
    {
        public bool IsDisabled
            => IsEnabled is false;

        public bool IsEnabled
            => ClientIsActive && IsServiceAvailable;

        public bool IsServiceUnavailable
            => !IsServiceAvailable;

        public bool IsServiceAvailable
            => Settings is { EmergenciesEnabled: true };

        [MemberNotNullWhen(true, nameof(ClientInfo))]
        public bool ClientIsActive
            => ClientInfo is { Active: true }
               && ClientStatus is { Blocked: false };

        [MemberNotNullWhen(true, nameof(Emergency))]
        public bool IsEmergencyInProgress
            => Emergency is { CompletionTimestamp: null };

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

        public async Task UpdateAsync(IMedRunnerApiClient api)
        {
            Errors.Clear();

            await api.WebSocket.EnsureInitializedAsync();
            // api.WebSocket.Events.EmergencyCreated += (_, emergency) => Emergency = emergency;
            // api.WebSocket.Events.EmergencyUpdated += (_, emergency) => Emergency = emergency;

            var settingsResponse = await api.OrgSettings.GetPublicSettingsAsync();
            if (settingsResponse.Success)
            {
                Settings = settingsResponse.Data;
                if (api.TokenProvider.IsAuthenticated)
                {
                    ClientIdentity = api.TokenProvider.Identity;
                }
            }
            else
            {
                Errors.Add(settingsResponse.ErrorMessage);
                return;
            }

            await UpdateClientAsync(api);
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
    }
}
