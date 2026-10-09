namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Common.Services;
using Domain.Abstractions.Game;
using Domain.Models;
using Domain.Models.Game;
using Domain.Models.Trade;

public abstract class PriceProviderBase : SelfInitializableServiceBase
{
    protected Bounds<PriceTag> CreateBoundsFrom<T>(ICollection<T> prices, Func<T, GameCurrency> priceSelector, PriceTag? fallback = null)
        where T : IGameEntityPrice
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
            CreatePriceTag(minDto, priceSelector, fallback),
            CreatePriceTag(maxDto, priceSelector, fallback),
            avgValue is not null
                ? new AggregatePriceTag(new GameCurrency(avgValue.Value))
                : fallback
        );
    }

    protected Bounds<PriceTag> CreateBoundsFrom(ICollection<PriceTag> prices, PriceTag? fallback = null)
    {
        var minPriceTag = prices.Min();
        var maxPriceTag = prices.Max();

        int? avgValue = maxPriceTag is BarePriceTag
            ? (int)prices.OfType<BarePriceTag>()
                .Select(priceTag => priceTag.Price)
                .Where(money => money.Amount > 0)
                .Average(money => money.Amount)
            : null;

        fallback ??= PriceTag.Unknown;
        return new Bounds<PriceTag>(
            minPriceTag ?? fallback,
            maxPriceTag ?? fallback,
            avgValue is not null
                ? new AggregatePriceTag(new GameCurrency(avgValue.Value))
                : fallback
        );
    }

    protected PriceTag CreatePriceTag(IGameEntityPurchasePrice? purchasePrice, PriceTag? fallback = null)
        => CreatePriceTag(purchasePrice, price => price.Price, fallback);

    protected PriceTag CreatePriceTag(IGameEntitySalePrice? salePrice, PriceTag? fallback = null)
        => CreatePriceTag(salePrice, price => price.Price, fallback);

    protected PriceTag CreatePriceTag(IGameEntityRentalPrice? rentalPrice, PriceTag? fallback = null)
        => CreatePriceTag(rentalPrice, price => price.Price, fallback);

    private PriceTag CreatePriceTag<T>(T? gameEntityPrice, Func<T, GameCurrency> selectPrice, PriceTag? fallback = null) where T : IGameEntityPrice
    {
        fallback ??= PriceTag.Unknown;
        if (gameEntityPrice is null)
        {
            return fallback;
        }

        var price = selectPrice(gameEntityPrice);
        return gameEntityPrice switch
        {
            _ when price.Amount is 0 => fallback,
            GameEntityTerminalPrice context => new KnownPriceTagWithLocation(price, context.Terminal, context.UpdatedAt),
            GameEntityPrice context => new KnownPriceTag(price, context.UpdatedAt),
            not null => new BarePriceTag(price),
            _ => fallback,
        };
    }
}
