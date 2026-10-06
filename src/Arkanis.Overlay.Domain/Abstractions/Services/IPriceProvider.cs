namespace Arkanis.Overlay.Domain.Abstractions.Services;

public interface IPriceProvider
    : IPurchasePriceProvider,
        ISalePriceProvider,
        IRentPriceProvider,
        IMarketPriceProvider;
