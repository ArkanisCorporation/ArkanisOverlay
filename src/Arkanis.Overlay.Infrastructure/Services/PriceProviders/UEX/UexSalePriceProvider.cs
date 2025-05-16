namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders.UEX;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;
using Domain.Models.Trade;

public class UexSalePriceProvider(
    ServiceDependencyResolver resolver,
    IGameCommodityPricingRepository commodityPriceRepository,
    IGameItemPurchasePricingRepository itemPriceRepository
) : UexPriceProviderBase, ISalePriceProvider
{
    public async ValueTask UpdatePriceTagAsync(IGameSellable gameEntity)
        => await (gameEntity switch
        {
            GameCommodity commodity => UpdateCommodityAsync(commodity),
            GameItem item => UpdateItemAsync(item),
            _ => ValueTask.CompletedTask,
        });

    public async ValueTask<List<PriceTag>> GetPriceTagsWithinAsync(IGameSellable gameEntity, IGameLocation? gameLocation)
        => gameEntity switch
        {
            GameCommodity commodity => await GetCommodityPriceTagsWithinAsync(commodity, gameLocation),
            GameItem item => await GetItemPriceTagsWithinAsync(item, gameLocation),
            _ => [],
        };

    public async ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGameSellable gameEntity, IGameLocation gameLocation)
        => gameEntity switch
        {
            GameCommodity commodity => await GetCommodityPriceTagAsync(commodity, gameLocation),
            GameItem item => await GetItemPriceTagAsync(item, gameLocation),
            _ => Bounds.All(PriceTag.Unknown),
        };

    private async ValueTask<List<PriceTag>> GetItemPriceTagsWithinAsync(GameItem gameEntity, IGameLocation? gameLocation)
    {
        var prices = await itemPriceRepository.GetPurchasePricesForItemAsync(gameEntity.Id);
        var pricesAtLocation = prices.Where(x => gameLocation?.IsOrContains(x.Terminal) ?? true).ToList();
        return pricesAtLocation.Select(price => CreatePriceTagFrom(price, x => x.PurchasePrice)).ToList();
    }

    private async ValueTask<List<PriceTag>> GetCommodityPriceTagsWithinAsync(GameCommodity gameEntity, IGameLocation? gameLocation)
    {
        var prices = await commodityPriceRepository.GetAllForCommodityAsync(gameEntity.Id);
        var pricesAtLocation = prices.Where(x => gameLocation?.IsOrContains(x.Terminal) ?? true).ToList();
        return pricesAtLocation.Select(price => CreatePriceTagFrom(price, x => x.PurchasePrice)).ToList();
    }

    private async ValueTask<Bounds<PriceTag>> GetItemPriceTagAsync(GameItem gameEntity, IGameLocation gameLocation)
    {
        var pricesAtLocation = await GetItemPriceTagsWithinAsync(gameEntity, gameLocation);
        return CreateBoundsFrom(pricesAtLocation, PriceTag.MissingFor(gameLocation));
    }

    private async ValueTask<Bounds<PriceTag>> GetCommodityPriceTagAsync(GameCommodity gameEntity, IGameLocation gameLocation)
    {
        var pricesAtLocation = await GetCommodityPriceTagsWithinAsync(gameEntity, gameLocation);
        return CreateBoundsFrom(pricesAtLocation, PriceTag.MissingFor(gameLocation));
    }

    private async ValueTask UpdateCommodityAsync(GameCommodity gameEntity)
    {
        var prices = await commodityPriceRepository.GetAllForCommodityAsync(gameEntity.Id);
        var priceBounds = CreateBoundsFrom(prices, price => price.SalePrice);
        gameEntity.UpdateSalePrices(priceBounds);
    }

    private async ValueTask UpdateItemAsync(GameItem gameEntity)
    {
        var prices = await itemPriceRepository.GetPurchasePricesForItemAsync(gameEntity.Id);
        var priceBounds = CreateBoundsFrom(prices, price => price.SalePrice);
        gameEntity.UpdateSalePrices(priceBounds);
    }

    protected override async Task InitializeAsyncCore(CancellationToken cancellationToken)
        => await resolver.DependsOn(this, commodityPriceRepository, itemPriceRepository)
            .WaitUntilReadyAsync(cancellationToken);
}
