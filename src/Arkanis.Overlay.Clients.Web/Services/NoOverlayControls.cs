namespace Arkanis.Overlay.Host.Server.Services;

using Domain.Abstractions.Services;

public class NoOverlayControls : IOverlayControls
{
    public ValueTask ShowAsync()
        => ValueTask.CompletedTask;

    public ValueTask HideAsync()
        => ValueTask.CompletedTask;
}
