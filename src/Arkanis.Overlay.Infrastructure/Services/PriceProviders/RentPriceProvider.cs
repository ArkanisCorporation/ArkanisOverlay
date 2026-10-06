namespace Arkanis.Overlay.Infrastructure.Services.PriceProviders;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Trade;
using Repositories.Local.Specialised;

public class RentPriceProvider(
    ServiceDependencyResolver resolver,
    GameRentalPricingRepositoryAggregate pricingRepositoryAggregate
) : PriceProviderBase, IRentPriceProvider
{
    public async ValueTask UpdatePriceTagAsync(IGameRentable gameEntity)
    {
        var bounds = await GetBoundsAsync(gameEntity, null);
        gameEntity.UpdateRentPrices(bounds);
    }

    public async ValueTask<ICollection<PriceTag>> GetPriceTagsWithinAsync(IGameRentable gameEntity, IGameLocation? gameLocation)
    {
        var prices = await pricingRepositoryAggregate.GetAllForAsync(gameEntity.Id);
        var filtered = gameLocation switch
        {
            not null => prices.Where(price => price is IGameLocatedAt locatedAt && gameLocation.IsOrContains(locatedAt.Location)),
            _ => prices,
        };

        return filtered
            .Select(price => CreatePriceTag(price))
            .OfType<BarePriceTag>()
            .OrderBy(x => x.Price)
            .ToArray();
    }

    public async ValueTask<Bounds<PriceTag>> GetPriceTagAtAsync(IGameRentable gameEntity, IGameLocation gameLocation)
        => await GetBoundsAsync(gameEntity, gameLocation);

    private async ValueTask<Bounds<PriceTag>> GetBoundsAsync(IGameRentable gameEntity, IGameLocation? gameLocation)
    {
        var fallback = gameLocation switch
        {
            not null => PriceTag.MissingFor(gameLocation),
            _ => PriceTag.Unknown,
        };

        var filtered = await GetPriceTagsWithinAsync(gameEntity, gameLocation);
        return CreateBoundsFrom(filtered, fallback);
    }

    protected override Task InitializeAsyncCore(CancellationToken cancellationToken)
        => resolver.DependsOn(this, pricingRepositoryAggregate)
            .WaitUntilReadyAsync(cancellationToken);
}
