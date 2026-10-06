#pragma warning disable CA1716
namespace Arkanis.Overlay.Components.Shared;
#pragma warning restore CA1716
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Overlay.External.MedRunner.API.Abstractions;
using Overlay.External.MedRunner.API.Endpoints.Emergency.Response;
using Overlay.External.MedRunner.Models;
using Overlay.Infrastructure.Services.External;

public abstract class MedRunnerComponentBase : ComponentBase, IDisposable
{
    protected string? EmergencyId
        => Emergency?.Id;

    public Emergency? Emergency
        => EmergencyContext.Emergency;

    [MemberNotNullWhen(true, nameof(Emergency), nameof(EmergencyId))]
    public bool HasEmergency
        => Emergency is not null;

    [MemberNotNullWhen(true, nameof(Emergency), nameof(EmergencyId))]
    public bool HasEmergencyInProgress
        => Emergency is { Status: MissionStatus.Pending or MissionStatus.Accepted };

    public IMedRunnerApiClient MedRunner
        => ServiceContext.ApiClient;

    [Inject]
    public required MedRunnerAccountContext ServiceContext { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public EventCallback<bool> IsLoadingChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public required EmergencyContextModel EmergencyContext { get; set; }

    [Parameter]
    public EventCallback<EmergencyContextModel> EmergencyContextChanged { get; set; }

    [Parameter]
    public string? ContentId { get; set; }

    public virtual void Dispose()
    {
        ServiceContext.Updated -= OnExternalUpdate;
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ServiceContext.Updated += OnExternalUpdate;
    }

    private void OnExternalUpdate(object? _, EventArgs e)
        => InvokeAsync(StateHasChanged);

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

    public sealed class EmergencyContextModel : IDisposable
    {
        private EventCallback<EmergencyContextModel> _callback;
        private MedRunnerAccountContext? _serviceContext;

        [MemberNotNullWhen(true, nameof(Emergency))]
        public bool IsEmergencyInProgress
            => Emergency is { CompletionTimestamp: null };

        public bool HasErrors
            => Errors is { Count: > 0 };

        public Emergency? Emergency { get; set; }
        public EmergencyDetailsDefaults? DetailsDefaults { get; set; }
        public TeamDetailsResponse? RespondingTeam { get; set; }

        public List<string> Errors { get; init; } = [];

        public void Dispose()
        {
            if (_serviceContext is null)
            {
                return;
            }

            _serviceContext.Events.EmergencyCreated -= OnEmergencyCreated;
            _serviceContext.Events.EmergencyUpdated -= OnEmergencyUpdated;
            _serviceContext = null;
        }

        public async Task EnsureInitializedAsync(
            MedRunnerAccountContext serviceContext,
            EventCallback<EmergencyContextModel> callback
        )
        {
            if (_serviceContext is not null)
            {
                return;
            }

            _callback = callback;
            _serviceContext = serviceContext;
            _serviceContext.Events.EmergencyUpdated += OnEmergencyUpdated;
            await Task.CompletedTask;
        }

        private void OnEmergencyCreated(object? _, Emergency emergency)
        {
            Emergency = emergency;
            SendUpdate();
        }

        private void OnEmergencyUpdated(object? _, Emergency emergency)
        {
            if (emergency.Id != Emergency?.Id)
            {
                return;
            }

            Emergency = emergency;
            SendUpdate();
        }

        private void SendUpdate()
            => _callback.InvokeAsync();
    }

    public sealed record EmergencyDetailsDefaults(string EmergencyId, string ExactLocation, string? LocationType, string? Remarks);
}
