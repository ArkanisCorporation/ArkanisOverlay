namespace Arkanis.Overlay.Application.Services;

using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWindowOverlayControls(this IServiceCollection services)
        => services.AddSingleton<IOverlayControls, WindowsOverlayControls>();
}
