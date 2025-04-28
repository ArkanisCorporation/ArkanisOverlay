namespace Arkanis.Overlay.Components.Helpers;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddJavaScriptEventInterop(this IServiceCollection services)
        => services.AddTransient<EventInterop>();
}
