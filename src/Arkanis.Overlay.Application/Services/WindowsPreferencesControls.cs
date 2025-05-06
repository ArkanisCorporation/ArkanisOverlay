namespace Arkanis.Overlay.Application.Services;

using Domain.Abstractions.Services;
using UI.Windows;

public class WindowsPreferencesControls
{
    public event EventHandler? PreferencesClosed;

    public ValueTask CloseAsync()
    {
        PreferencesClosed?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }
}
