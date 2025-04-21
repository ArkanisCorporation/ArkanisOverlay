namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    private static readonly Type ReferenceSourceType = typeof(DependencyInjection);

    public static IServiceCollection AddPriceProviderServices(this IServiceCollection services)
        => services.Scan(scan => scan
            .FromAssembliesOf(ReferenceSourceType)
            .AddClasses(type => type.InNamespaceOf(ReferenceSourceType), false)
            .AsImplementedInterfaces()
        );
}
