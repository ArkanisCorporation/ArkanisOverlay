namespace Arkanis.Overlay.Host.Desktop.Services;

using System.Windows;

public class WindowControls<T> where T : Window
{
    private readonly HashSet<T> _windows = [];

    public async ValueTask CloseAsync()
    {
        foreach (var window in _windows)
        {
            await window.Dispatcher.InvokeAsync(() => window.Close());
        }
    }

    public void Bind(T window)
        => _windows.Add(window);

    public void Unbind(T window)
        => _windows.Remove(window);
}
