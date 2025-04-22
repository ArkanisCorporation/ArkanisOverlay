namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders.UEX;

using Abstractions;
using Common.Extensions;
using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddUexPriceProviderServices(this IServiceCollection services)
        => services
            .AddSingleton<UexPurchasePriceProvider>()
            .Alias<IPurchasePriceProvider, UexPurchasePriceProvider>()
            .Alias<ISelfInitializable, UexPurchasePriceProvider>()
            .AddSingleton<UexSellPriceProvider>()
            .Alias<ISellPriceProvider, UexSellPriceProvider>()
            .Alias<ISelfInitializable, UexSellPriceProvider>()
            .AddSingleton<UexRentPriceProvider>()
            .Alias<IRentPriceProvider, UexRentPriceProvider>()
            .Alias<ISelfInitializable, UexRentPriceProvider>();
}
