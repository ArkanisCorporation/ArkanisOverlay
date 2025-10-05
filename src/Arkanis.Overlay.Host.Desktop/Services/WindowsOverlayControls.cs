namespace Arkanis.Overlay.Host.Desktop.Services;

using Domain.Abstractions.Services;
using UI.Windows;

public class WindowsOverlayControls : IOverlayControls, IOverlayEventProvider, IOverlayEventControls
{
    public Task ForceShowAsync()
    {
        // TODO: Implement logic for forcing the overlay to be shown.
        return Task.CompletedTask;
    }

    public ValueTask ShowAsync()
    {
        OverlayWindow.Instance?.Show();
        OverlayShown?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public ValueTask HideAsync()
    {
        OverlayWindow.Instance?.Hide();
        OverlayHidden?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    public void SetBlurEnabled(bool isEnabled)
    {
    }

    public void Shutdown()
        => OverlayWindow.Instance?.Exit();

    public void OnFocusGained()
        => OverlayFocused?.Invoke(this, EventArgs.Empty);

    public void OnFocusLost()
        => OverlayBlurred?.Invoke(this, EventArgs.Empty);

    public void OnOverlayWindowShown()
        => OverlayShown?.Invoke(this, EventArgs.Empty);

    public void OnOverlayWindowHidden()
        => OverlayHidden?.Invoke(this, EventArgs.Empty);

    public void OnGameWindowFound()
        => GameWindowFound?.Invoke(this, EventArgs.Empty);

    public void OnGameWindowLost()
        => GameWindowLost?.Invoke(this, EventArgs.Empty);

    public void OnGameWindowFocused()
        => GameWindowFocused?.Invoke(this, EventArgs.Empty);

    public void OnGameWindowBlurred()
        => GameWindowBlurred?.Invoke(this, EventArgs.Empty);

    public event EventHandler? OverlayShown;
    public event EventHandler? OverlayHidden;

    public event EventHandler? OverlayFocused;
    public event EventHandler? OverlayBlurred;
    public event EventHandler? GameWindowFocused;
    public event EventHandler? GameWindowBlurred;

    public event EventHandler? GameWindowFound;
    public event EventHandler? GameWindowLost;
}
