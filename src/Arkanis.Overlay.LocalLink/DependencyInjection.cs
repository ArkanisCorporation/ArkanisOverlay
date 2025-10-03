namespace Arkanis.Overlay.LocalLink;

using Abstractions;
using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Services;

public static class DependencyInjection
{
    public static IServiceCollection AddLocalLinkSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<CustomProtocolClient>();

        services.AddHostedService<NamedPipeCommandServerBackgroundPublisherService>();

        return services;
    }

    public static IServiceCollection AddLocalLinkHostServices<T>(this IServiceCollection services)
        where T : class, ILocalLinkCommandPublisher
    {
        services.AddSingleton<T>()
            .Alias<ILocalLinkCommandPublisher, T>();

        services.AddSingleton<NamedPipeCommandClient>()
            .AddSingleton<NamedPipeCommandServer>()
            .Alias<NamedPipeCommandServer, NamedPipeCommandServer>();

        return services;
    }
}
