namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders.UEX;

using Common;
using Domain.Abstractions.Game;
using Domain.Models;
using Domain.Models.Trade;
using External.UEX.Abstractions;
using NodaMoney;

public abstract class UexPriceProviderBase : SelfInitializableServiceBase
{
    protected Bounds<PriceTag> CreateBoundFrom(ICollection<CommodityPriceBriefDTO>? commodityPrices, Func<CommodityPriceBriefDTO?, double?> priceSelector)
        => CreateBoundFrom(commodityPrices ?? [], priceSelector, GetLocationFor, GetUpdateTimeFor);

    protected Bounds<PriceTag> CreateBoundFrom(ICollection<ItemPriceBriefDTO>? itemPrices, Func<ItemPriceBriefDTO?, double?> priceSelector)
        => CreateBoundFrom(itemPrices ?? [], priceSelector, GetLocationFor, GetUpdateTimeFor);

    protected Bounds<PriceTag> CreateBoundFrom(
        ICollection<VehiclePurchasePriceBriefDTO>? vehiclePrices,
        Func<VehiclePurchasePriceBriefDTO?, double?> priceSelector
    )
        => CreateBoundFrom(vehiclePrices ?? [], priceSelector, GetLocationFor, GetUpdateTimeFor);

    protected Bounds<PriceTag> CreateBoundFrom(ICollection<VehicleRentalPriceBriefDTO>? vehiclePrices, Func<VehicleRentalPriceBriefDTO?, double?> priceSelector)
        => CreateBoundFrom(vehiclePrices ?? [], priceSelector, GetLocationFor, GetUpdateTimeFor);

    private static Bounds<PriceTag> CreateBoundFrom<T>(
        ICollection<T> prices,
        Func<T?, double?> priceSelector,
        Func<T?, IGameLocation> resolveLocation,
        Func<T?, DateTimeOffset> resolveUpdateTime
    )
    {
        var minDto = prices.MinBy(priceSelector);
        var minPrice = new Money(priceSelector(minDto) ?? 0, ApplicationConstants.GameCurrency);

        var maxDto = prices.MaxBy(priceSelector);
        var maxPrice = new Money(priceSelector(maxDto) ?? 0, ApplicationConstants.GameCurrency);

        var avgValue = prices.Average(priceSelector);
        var avgPrice = new Money(avgValue ?? 0, ApplicationConstants.GameCurrency);

        return new Bounds<PriceTag>(
            minDto is not null
                ? new KnownPriceTag(minPrice, resolveLocation(minDto), resolveUpdateTime(minDto))
                : PriceTag.Unknown,
            maxDto is not null
                ? new KnownPriceTag(maxPrice, resolveLocation(maxDto), resolveUpdateTime(maxDto))
                : PriceTag.Unknown,
            avgValue is not null
                ? new AggregatePriceTag(avgPrice)
                : PriceTag.Unknown
        );
    }

    private IGameLocation GetLocationFor(CommodityPriceBriefDTO? priceDTO)
        => throw new NotImplementedException();

    private IGameLocation GetLocationFor(ItemPriceBriefDTO? priceDTO)
        => throw new NotImplementedException();

    private IGameLocation GetLocationFor(VehiclePurchasePriceBriefDTO? priceDTO)
        => throw new NotImplementedException();

    private IGameLocation GetLocationFor(VehicleRentalPriceBriefDTO? priceDTO)
        => throw new NotImplementedException();

    private DateTimeOffset GetUpdateTimeFor(CommodityPriceBriefDTO? priceDTO)
        => CreateDateTimeFromUnixTimestamp(priceDTO?.Date_modified ?? 0);

    private DateTimeOffset GetUpdateTimeFor(ItemPriceBriefDTO? priceDTO)
        => CreateDateTimeFromUnixTimestamp(priceDTO?.Date_modified ?? 0);

    private DateTimeOffset GetUpdateTimeFor(VehiclePurchasePriceBriefDTO? priceDTO)
        => CreateDateTimeFromUnixTimestamp(priceDTO?.Date_modified ?? 0);

    private DateTimeOffset GetUpdateTimeFor(VehicleRentalPriceBriefDTO? priceDTO)
        => CreateDateTimeFromUnixTimestamp(priceDTO?.Date_modified ?? 0);

    private static DateTimeOffset CreateDateTimeFromUnixTimestamp(double updateTimestamp)
        => DateTimeOffset.FromUnixTimeSeconds((long)updateTimestamp);
}
