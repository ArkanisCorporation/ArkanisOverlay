namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;
using Domain.Models.Trade;
using Repositories.Local.Specialised;

public class MarketplacePriceProvider(
    ServiceDependencyResolver resolver,
    GameMarketPricingRepositoryAggregate pricingRepositoryAggregate
) : PriceProviderBase, IMarketPriceProvider
{
    public async ValueTask<Bounds<PriceTag>> GetMarketPriceTagAsync(IGameEntity gameEntity)
    {
        var prices = await pricingRepositoryAggregate.GetAllForAsync(gameEntity.Id);
        return CreateBoundsFrom(
            prices,
            price => price switch
            {
                GameEntityMarketPurchasePrice purchase => purchase.PurchasePrice,
                GameEntityMarketSalePrice sale => sale.SalePrice,
                _ => throw new NotSupportedException(),
            }
        );
    }

    protected override Task InitializeAsyncCore(CancellationToken cancellationToken)
        => resolver.DependsOn(this, pricingRepositoryAggregate)
            .WaitUntilReadyAsync(cancellationToken);
}
