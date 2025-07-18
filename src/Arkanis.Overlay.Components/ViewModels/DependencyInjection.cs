namespace Arkanis.Overlay.Components.ViewModels;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddComponentViewModels(this IServiceCollection services)
        => services.AddTransient<InventoryViewModel>();
}
