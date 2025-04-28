namespace Arkanis.Overlay.Components.Services;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddKeyboardProxyService(this IServiceCollection services)
        => services.AddScoped<KeyboardProxy>();
}
