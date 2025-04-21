namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Enums;
using Domain.Models.Game;
using Domain.Models.Trade;
using External.UEX.Abstractions;

public class UexPurchasePriceProvider(
    IUexCommoditiesApi commoditiesApi,
    IUexItemsApi itemsApi,
    IUexVehiclesApi vehiclesApi
) : UexPriceProviderBase, IPurchasePriceProvider
{
    private Dictionary<UexApiGameEntityId, ICollection<CommodityPriceBriefDTO>> _commodityPrices = [];
    private Dictionary<UexApiGameEntityId, ICollection<ItemPriceBriefDTO>> _itemPrices = [];
    private Dictionary<UexApiGameEntityId, ICollection<VehiclePurchasePriceBriefDTO>> _vehiclePrices = [];

    public ValueTask UpdatePriceTagAsync(IGamePurchasable gameEntity)
    {
        if (gameEntity.EntityCategory is GameEntityCategory.Commodity)
        {
            UpdateCommodity(gameEntity);
        }
        else if (gameEntity.EntityCategory is GameEntityCategory.Item)
        {
            UpdateItem(gameEntity);
        }
        else if (gameEntity.EntityCategory is GameEntityCategory.GroundVehicle or GameEntityCategory.SpaceShip)
        {
            UpdateVehicle(gameEntity);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask<PriceTag> GetPriceTagAtAsync(IGamePurchasable gameEntity, IGameLocation gameLocation)
        => ValueTask.FromResult(PriceTag.Unknown);

    private void UpdateCommodity(IGamePurchasable gameEntity)
    {
        if (!_commodityPrices.TryGetValue(gameEntity.Id, out var prices))
        {
            return;
        }

        var priceBounds = CreateBoundFrom(prices, price => price?.Price_buy);
        gameEntity.UpdatePurchasePrices(priceBounds);
    }

    private void UpdateItem(IGamePurchasable gameEntity)
    {
        if (!_itemPrices.TryGetValue(gameEntity.Id, out var prices))
        {
            return;
        }

        var priceBounds = CreateBoundFrom(prices, price => price?.Price_buy);
        gameEntity.UpdatePurchasePrices(priceBounds);
    }

    private void UpdateVehicle(IGamePurchasable gameEntity)
    {
        if (!_vehiclePrices.TryGetValue(gameEntity.Id, out var prices))
        {
            return;
        }

        var priceBounds = CreateBoundFrom(prices, price => price?.Price_buy);
        gameEntity.UpdatePurchasePrices(priceBounds);
    }

    protected override async Task InitializeAsyncCore(CancellationToken cancellationToken)
    {
        if (this is { _commodityPrices.Count: > 0 } or { _vehiclePrices.Count: > 0 } or { _itemPrices.Count: > 0 })
        {
            return;
        }

        var itemPricesResponse = await itemsApi.GetItemsPricesAllAsync(cancellationToken).ConfigureAwait(false);
        var itemPrices = itemPricesResponse.Result.Data ?? [];
        _itemPrices = itemPrices
            .GroupBy(x => UexApiGameEntityId.Create<GameItem>(x.Id_item ?? 0))
            .ToDictionary(UexApiGameEntityId (x) => x.Key, ICollection<ItemPriceBriefDTO> (x) => x.ToArray());

        var commodityPricesResponse = await commoditiesApi.GetCommoditiesPricesAllAsync(cancellationToken).ConfigureAwait(false);
        var commodityPrices = commodityPricesResponse.Result.Data ?? [];
        _commodityPrices = commodityPrices
            .GroupBy(x => UexApiGameEntityId.Create<GameCommodity>(x.Id_commodity ?? 0))
            .ToDictionary(UexApiGameEntityId (x) => x.Key, ICollection<CommodityPriceBriefDTO> (x) => x.ToArray());

        var vehiclePricesResponse = await vehiclesApi.GetVehiclesPurchasesPricesAllAsync(cancellationToken).ConfigureAwait(false);
        var vehiclePrices = vehiclePricesResponse.Result.Data ?? [];
        _vehiclePrices = vehiclePrices
            .GroupBy(x => UexApiGameEntityId.Create<GameVehicle>(x.Id_vehicle ?? 0))
            .ToDictionary(UexApiGameEntityId (x) => x.Key, ICollection<VehiclePurchasePriceBriefDTO> (x) => x.ToArray());
    }
}
