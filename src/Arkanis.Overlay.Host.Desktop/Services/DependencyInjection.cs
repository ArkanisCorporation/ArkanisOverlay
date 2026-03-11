namespace Arkanis.Overlay.Host.Desktop.Services;

using Common.Extensions;
using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class DependencyInjection
{
    public static IServiceCollection AddWindowsOverlayControls(this IServiceCollection services)
        => services.AddSingleton<WindowsOverlayControls>()
            .Alias<IOverlayControls, WindowsOverlayControls>()
            .Alias<IOverlayEventProvider, WindowsOverlayControls>()
            .Alias<IOverlayEventControls, WindowsOverlayControls>();

    public static IServiceCollection AddOcrServices(this IServiceCollection services)
        => services.AddSingleton<GameScreenProvider>()
            .AddSingleton<GameScreenProcessor>()
            .Alias<IHostedService, GameScreenProcessor>();
}
