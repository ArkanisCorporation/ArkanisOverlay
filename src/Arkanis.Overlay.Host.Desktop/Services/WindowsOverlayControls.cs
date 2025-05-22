namespace Arkanis.Overlay.Host.Desktop.Services;

using Domain.Abstractions.Services;
using UI.Windows;

public class WindowsOverlayControls : IOverlayControls, IOverlayEventProvider, IOverlayEventControls
{
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

    public void SetFocus(bool isFocused = true)
    {
        if (isFocused)
        {
            OverlayFocused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            OverlayBlurred?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetGameConnected(bool isConnected = true)
    {
        if (isConnected)
        {
            GameConnected?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            GameDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? OverlayShown;
    public event EventHandler? OverlayHidden;

    public event EventHandler? OverlayFocused;
    public event EventHandler? OverlayBlurred;

    public event EventHandler? GameConnected;
    public event EventHandler? GameDisconnected;
}
