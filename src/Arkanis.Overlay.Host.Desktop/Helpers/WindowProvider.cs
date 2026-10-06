namespace Arkanis.Overlay.Host.Desktop.Helpers;

using System.Windows;
using Microsoft.Extensions.DependencyInjection;

public sealed class WindowProvider<T>(IServiceProvider provider) where T : Window
{
    public T GetWindow()
        => provider.GetRequiredService<T>();
}
