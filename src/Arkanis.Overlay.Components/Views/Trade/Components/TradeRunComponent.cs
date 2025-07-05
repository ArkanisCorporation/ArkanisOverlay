namespace Arkanis.Overlay.Components.Views.Trade.Components;

using Domain.Abstractions.Services;
using Domain.Models.Trade;
using Microsoft.AspNetCore.Components;

public class TradeRunComponent : ComponentBase
{
    [Inject]
    public required ITradeRunManager TradeRunManager { get; set; }

    [Parameter]
    [EditorRequired]
    public required TradeRun TradeRun { get; set; }

    [Parameter]
    public EventCallback<TradeRun> TradeRunChanged { get; set; }

    protected async Task UpdateRunAsync()
        => await TradeRunChanged.InvokeAsync(TradeRun);
}
