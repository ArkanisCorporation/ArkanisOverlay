namespace Arkanis.Overlay.Components.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public sealed class EventInterop(IJSRuntime jsRuntime) : IDisposable
{
    private readonly List<DotNetObjectReference<Handler>> _handlers = [];

    public void Dispose()
    {
        lock (_handlers)
        {
            foreach (var handler in _handlers)
            {
                handler.Dispose();
            }

            _handlers.Clear();
        }
    }

    public async Task RegisterWindowEventHandlerAsync(string eventName, Handler handler)
    {
        var reference = CreateHandlerReference(handler);
        await jsRuntime.InvokeVoidAsync("addWindowEventListener", eventName, reference);
    }

    public async Task RegisterDocumentEventHandlerAsync(string eventName, Handler handler)
    {
        var reference = CreateHandlerReference(handler);
        await jsRuntime.InvokeVoidAsync("addDocumentEventListener", eventName, reference);
    }

    public async Task RegisterDocumentEventHandlerAsync(ElementReference element, string eventName, Handler handler)
    {
        var reference = CreateHandlerReference(handler);
        await jsRuntime.InvokeVoidAsync("addElementEventListener", element, eventName, reference);
    }

    private DotNetObjectReference<Handler> CreateHandlerReference(Handler handler)
    {
        var reference = DotNetObjectReference.Create(handler);
        lock (_handlers)
        {
            _handlers.Add(reference);
        }

        return reference;
    }

    public static Handler CreateHandler(Func<EventArgs, Task> callback)
        => new(async args => await callback(args));

    public static Handler CreateHandler(Func<Task> callback)
        => new(async _ => await callback());

    public static Handler CreateHandler(Action callback)
        => new(_ =>
            {
                callback();
                return ValueTask.CompletedTask;
            }
        );

    public sealed class Handler(Func<EventArgs, ValueTask> callback)
    {
        [JSInvokable]
        public async Task HandleEvent(EventArgs args)
            => await callback(args);
    }
}
