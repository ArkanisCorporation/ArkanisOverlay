namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders.UEX;

using Domain.Models;
using Domain.Models.Game;
using Domain.Models.Trade;

public abstract class UexPriceProviderBase : SelfInitializableServiceBase
{
    protected static Bounds<PriceTag> CreateBoundsFrom<T>(ICollection<T> prices, Func<T, GameCurrency> priceSelector, PriceTag? fallback = null)
        where T : GameEntityPricing
    {
        var minDto = prices.Where(dto => priceSelector(dto).Amount > 0).MinBy(priceSelector);
        var maxDto = prices.Where(dto => priceSelector(dto).Amount > 0).MaxBy(priceSelector);

        int? avgValue = maxDto is not null
            ? (int)prices.Select(priceSelector)
                .Where(money => money.Amount > 0)
                .Average(money => money.Amount)
            : null;

        fallback ??= PriceTag.Unknown;
        return new Bounds<PriceTag>(
            CreatePriceTagFrom(minDto, priceSelector, fallback),
            CreatePriceTagFrom(maxDto, priceSelector, fallback),
            avgValue is not null
                ? new AggregatePriceTag(new GameCurrency(avgValue.Value))
                : fallback
        );
    }

    protected static Bounds<PriceTag> CreateBoundsFrom(ICollection<PriceTag> prices, PriceTag? fallback = null)
    {
        fallback ??= PriceTag.Unknown;
        var minDto = prices.Min() ?? fallback;
        var maxDto = prices.Max() ?? fallback;

        var pricesWithValue = prices.OfType<PriceTagWithValue>()
            .Where(x => x.Price.Amount > 0)
            .ToList();

        var avgValue = pricesWithValue is { Count: > 0 }
            ? (int?)pricesWithValue.Average(x => x.Price.Amount)
            : null;

        return new Bounds<PriceTag>(
            minDto,
            maxDto,
            avgValue is not null
                ? new AggregatePriceTag(new GameCurrency(avgValue.Value))
                : fallback
        );
    }

    protected static PriceTag CreatePriceTagFrom<T>(T? price, Func<T, GameCurrency> priceSelector, PriceTag? fallback = null)
        where T : GameEntityPricing
    {
        fallback ??= PriceTag.Unknown;
        if (price is null)
        {
            return fallback;
        }

        var priceValue = priceSelector(price);
        return priceValue.Amount > 0
            ? new KnownPriceTag(priceValue, price.Terminal, price.UpdatedAt)
            : fallback;
    }
}
