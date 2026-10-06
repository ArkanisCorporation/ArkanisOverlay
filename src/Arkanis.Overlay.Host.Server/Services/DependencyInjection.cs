namespace Arkanis.Overlay.Host.Server.Services;

using Common.Extensions;
using Domain.Abstractions.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServerOverlayControls(this IServiceCollection services)
        => services.AddScoped<WebOverlayControls>()
            .Alias<IOverlayEventProvider, WebOverlayControls>(ServiceLifetime.Scoped)
            .Alias<IOverlayEventControls, WebOverlayControls>(ServiceLifetime.Scoped)
            .Alias<IOverlayControls, WebOverlayControls>(ServiceLifetime.Scoped);
}
