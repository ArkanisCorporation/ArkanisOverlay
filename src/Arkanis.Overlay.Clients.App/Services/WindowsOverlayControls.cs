namespace Arkanis.Overlay.Application.Services;

using Domain.Abstractions.Services;
using UI.Windows;

public class WindowsOverlayControls : IOverlayControls
{
    public ValueTask ShowAsync()
    {
        OverlayWindow.Instance?.Show();
        return ValueTask.CompletedTask;
    }

    public ValueTask HideAsync()
    {
        OverlayWindow.Instance?.Hide();
        return ValueTask.CompletedTask;
    }
}
