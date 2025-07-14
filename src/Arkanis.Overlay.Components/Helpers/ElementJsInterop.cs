namespace Arkanis.Overlay.Components.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class ElementJsInterop
{
    private const string InvokeOnElementInteropMethodName = "invokeOnElement";
    private IJSRuntime? _jsRuntime;

    public async Task InvokeVoidAsync(ElementReference element, string methodIdentifier, params object[] args)
    {
        if (_jsRuntime is not null)
        {
            await _jsRuntime.InvokeVoidAsync(InvokeOnElementInteropMethodName, element, methodIdentifier, args);
        }
    }

    public async Task InvokeAsync<T>(ElementReference element, string methodIdentifier, params object[] args)
    {
        if (_jsRuntime is not null)
        {
            await _jsRuntime.InvokeAsync<T>(InvokeOnElementInteropMethodName, element, methodIdentifier, args);
        }
    }

    public void Connect(IJSRuntime jsRuntime)
        => _jsRuntime = jsRuntime;

    public async Task ScrollToBottomAsync(ElementReference element)
        => await InvokeVoidAsync(
            element,
            "scrollTo",
            new
            {
                top = 1_000_000,
                left = 0,
                behavior = "smooth",
            }
        );
}
