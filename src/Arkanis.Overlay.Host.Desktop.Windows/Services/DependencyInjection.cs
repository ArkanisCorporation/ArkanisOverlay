namespace Arkanis.Overlay.Host.Desktop.Windows.Services;

using Microsoft.Extensions.DependencyInjection;
using Native;
using Overlay.UI.Native;

public static class DependencyInjection
{
    public static IServiceCollection AddNativeServices(this IServiceCollection services)
        => services.AddSingleton<IWindowUtils, WindowUtils>();
}
