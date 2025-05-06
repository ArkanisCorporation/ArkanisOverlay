namespace Arkanis.Overlay.Application.Services.Factories;

using Microsoft.Extensions.DependencyInjection;
using UI.Windows;

public class PreferencesWindowFactory(IServiceProvider serviceProvider)
{
    public PreferencesWindow CreateWindow()
        => ActivatorUtilities.CreateInstance<PreferencesWindow>(serviceProvider);
}
