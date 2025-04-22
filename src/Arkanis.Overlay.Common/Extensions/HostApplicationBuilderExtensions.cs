namespace Arkanis.Common.Extensions;

using Abstractions;
using Microsoft.Extensions.Hosting;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddConfiguration<T>(this IHostApplicationBuilder builder) where T : class, ISelfBindableOptions
    {
        builder.Services.AddConfiguration<T>(builder.Configuration);
        return builder;
    }
}
