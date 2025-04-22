namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders.UEX;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    private static readonly Type ReferenceSourceType = typeof(DependencyInjection);

    public static IServiceCollection AddUexPriceProviderServices(this IServiceCollection services)
        => services.Scan(scan => scan
            .FromAssembliesOf(ReferenceSourceType)
            .AddClasses(type => type.InNamespaceOf(ReferenceSourceType), false)
            .AsImplementedInterfaces()
        );
}
