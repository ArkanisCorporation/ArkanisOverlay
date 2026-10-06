namespace Arkanis.Overlay.Host.Desktop.Services;

using Common.Extensions;
using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWindowsOverlayControls(this IServiceCollection services)
        => services.AddSingleton<WindowsOverlayControls>()
            .Alias<IOverlayControls, WindowsOverlayControls>()
            .Alias<IOverlayEventProvider, WindowsOverlayControls>()
            .Alias<IOverlayEventControls, WindowsOverlayControls>();
}
