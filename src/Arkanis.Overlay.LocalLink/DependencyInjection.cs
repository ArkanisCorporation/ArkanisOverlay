namespace Arkanis.Overlay.LocalLink;

using Abstractions;
using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Services;

public static class DependencyInjection
{
    public static IServiceCollection AddLocalLinkSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<CustomProtocolClient>()
            .AddSingleton<LocalLinkCallHandler>();

        return services;
    }

    public static IServiceCollection AddLocalLinkHostServices<T>(this IServiceCollection services)
        where T : class, ILocalLinkCommandPublisher
    {
        services.AddSingleton<T>()
            .Alias<ILocalLinkCommandPublisher, T>();

        services.AddSingleton<NamedPipeCommandClient>()
            .AddSingleton<NamedPipeCommandServer>()
            .AddSingleton<NamedPipeCommandCallForwarder>();

        services.AddHostedService<NamedPipeCommandServerBackgroundPublisherService>();
        services.AddHostedService<LocalLinkCallHandler.HostedService>();

        return services;
    }
}
