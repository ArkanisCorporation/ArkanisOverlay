namespace Arkanis.Overlay.Application.Services;

using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWindowsOverlayControls(this IServiceCollection services)
        => services.AddSingleton<IOverlayControls, WindowsOverlayControls>();

    public static IServiceCollection AddPreferenceServiceCollection(this IServiceCollection services)
        => services.AddSingleton<WindowsPreferencesControls>();
}
