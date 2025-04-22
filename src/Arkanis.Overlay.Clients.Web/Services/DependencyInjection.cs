namespace Arkanis.Overlay.Host.Server.Services;

using Domain.Abstractions.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServerOverlayControls(this IServiceCollection services)
        => services.AddScoped<IOverlayControls, NoOverlayControls>();
}
