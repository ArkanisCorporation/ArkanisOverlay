namespace Arkanis.Overlay.Components.Helpers;

using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddJavaScriptEventInterop(this IServiceCollection services)
        => services.AddTransient<EventInterop>();

    public static IServiceCollection AddIconPickerBridge(this IServiceCollection services)
        => services
            .AddSingleton<IconPicker>()
            .AddSingleton<IIconPicker>(sp => sp.GetRequiredService<IconPicker>());
}
