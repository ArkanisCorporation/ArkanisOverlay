namespace Arkanis.Overlay.Host.Desktop.Services.Factories;

using System.Windows;
using Microsoft.Extensions.DependencyInjection;

public class WindowFactory(IServiceProvider serviceProvider)
{
    public T CreateWindow<T>() where T : Window
        => ActivatorUtilities.CreateInstance<T>(serviceProvider);
}
