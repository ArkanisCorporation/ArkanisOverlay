namespace Arkanis.Overlay.Components.Views.Trade.Components;

using Domain.Models.Trade;
using Microsoft.AspNetCore.Components;
using MudBlazor;

public class TradeRunStageComponent<T> : TradeRunComponent where T : TradeRun.Stage
{
    [CascadingParameter]
    internal MudStepper? StepperRef { get; set; }

    [Parameter]
    [EditorRequired]
    public required T Stage { get; set; }

    [Parameter]
    public EventCallback<T> StageChanged { get; set; }

    protected virtual async Task UpdateStageAsync()
        => await StageChanged.InvokeAsync(Stage);

    protected async Task NextStepAsync()
    {
        if (StepperRef is not null)
        {
            await StepperRef.NextStepAsync();
            await UpdateStageAsync();
        }
    }
}
