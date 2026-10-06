namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Common.Abstractions.Services;
using Common.Extensions;
using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPriceProviders(this IServiceCollection services)
        => services
            .AddSingleton<IPriceProvider, PriceProviderAggregate>()
            .AddPriceProviderServices();

    private static IServiceCollection AddPriceProviderServices(this IServiceCollection services)
        => services
            .AddSingleton<MarketplacePriceProvider>()
            .Alias<IMarketPriceProvider, MarketplacePriceProvider>()
            .Alias<ISelfInitializable, MarketplacePriceProvider>()
            .AddSingleton<PurchasePriceProvider>()
            .Alias<IPurchasePriceProvider, PurchasePriceProvider>()
            .Alias<ISelfInitializable, PurchasePriceProvider>()
            .AddSingleton<SalePriceProvider>()
            .Alias<ISalePriceProvider, SalePriceProvider>()
            .Alias<ISelfInitializable, SalePriceProvider>()
            .AddSingleton<RentPriceProvider>()
            .Alias<IRentPriceProvider, RentPriceProvider>()
            .Alias<ISelfInitializable, RentPriceProvider>();
}
