namespace Arkanis.Common.Extensions;

using Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguration<T>(this IServiceCollection services, IConfiguration configuration) where T : class, ISelfBindableOptions
        => services.Configure<T>(instance => instance.Bind(configuration));
}
