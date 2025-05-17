namespace Arkanis.Overlay.Host.Server.Services;

using Common.Extensions;
using Domain.Abstractions.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServerOverlayControls(this IServiceCollection services)
        => services.AddTransient<WebOverlayControls>()
            .Alias<IOverlayEventProvider, WebOverlayControls>()
            .Alias<IOverlayEventControls, WebOverlayControls>()
            .Alias<IOverlayControls, WebOverlayControls>();
}
