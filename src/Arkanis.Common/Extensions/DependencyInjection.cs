namespace Arkanis.Common.Extensions;

using Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration<T>(this IServiceCollection services, IConfiguration configuration) where T : class, ISelfBindableOptions
        => services.Configure<T>(instance => instance.Bind(configuration));

    public static IServiceCollection Alias<TAlias, TService>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService : TAlias
    {
        services.Add(ServiceDescriptor.Describe(typeof(TAlias), provider => provider.GetRequiredService(typeof(TService)), lifetime));
        return services;
    }
}
