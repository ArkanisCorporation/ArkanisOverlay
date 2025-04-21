namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Enums;
using Domain.Models.Game;
using Domain.Models.Trade;
using External.UEX.Abstractions;

public class UexSellPriceProvider(IUexCommoditiesApi commoditiesApi) : UexPriceProviderBase, ISellPriceProvider
{
    private Dictionary<UexApiGameEntityId, ICollection<CommodityPriceBriefDTO>> _commodityPrices = [];

    public ValueTask UpdatePriceTagAsync(IGameSellable gameEntity)
    {
        if (gameEntity.EntityCategory is GameEntityCategory.Commodity)
        {
            UpdateCommodity(gameEntity);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask<PriceTag> GetPriceTagAtAsync(IGameSellable gameEntity, IGameLocation gameLocation)
        => ValueTask.FromResult(PriceTag.Unknown);

    private void UpdateCommodity(IGameSellable gameEntity)
    {
        if (!_commodityPrices.TryGetValue(gameEntity.Id, out var prices))
        {
            return;
        }

        var priceBounds = CreateBoundFrom(prices, price => price?.Price_sell);
        gameEntity.UpdateSellPrices(priceBounds);
    }

    protected override async Task InitializeAsyncCore(CancellationToken cancellationToken)
    {
        if (this is { _commodityPrices.Count: > 0 })
        {
            return;
        }

        var commodityPricesResponse = await commoditiesApi.GetCommoditiesPricesAllAsync(cancellationToken).ConfigureAwait(false);
        var commodityPrices = commodityPricesResponse.Result.Data ?? [];
        _commodityPrices = commodityPrices
            .GroupBy(x => UexApiGameEntityId.Create<GameCommodity>(x.Id_commodity ?? 0))
            .ToDictionary(UexApiGameEntityId (x) => x.Key, ICollection<CommodityPriceBriefDTO> (x) => x.ToArray());
    }
}
