namespace Arkanis.Overlay.Infrastructure.Repositories.Local.Specialised;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;

internal class UexTradePricingRepositorySpecialisationDecorator(IGameEntityRepository<GameEntityTradePrice> repository)
    : UexPricingRepositorySpecialisationDecorator<GameEntityTradePrice>(repository),
        IGamePurchasePricingRepository,
        IGameSalePricingRepository
{
    public IAsyncEnumerable<IGameEntityPurchasePrice> GetAllPurchasePricesAsync(CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAllAsync(cancellationToken)
            .OfType<IGameEntityPurchasePrice>();

    public async ValueTask<ICollection<IGameEntityPurchasePrice>> GetPurchasePricesForAsync(IDomainId domainId, CancellationToken cancellationToken)
    {
        var items = await GetAllForOwnerAsync(domainId, cancellationToken);
        return items.OfType<IGameEntityPurchasePrice>().ToArray();
    }

    public IAsyncEnumerable<IGameEntitySalePrice> GetAllSalePricesAsync(CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAllAsync(cancellationToken)
            .OfType<IGameEntitySalePrice>();

    public async ValueTask<ICollection<IGameEntitySalePrice>> GetSalePricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default)
    {
        var items = await GetAllForOwnerAsync(domainId, cancellationToken);
        return items.OfType<IGameEntitySalePrice>().ToArray();
    }
}
