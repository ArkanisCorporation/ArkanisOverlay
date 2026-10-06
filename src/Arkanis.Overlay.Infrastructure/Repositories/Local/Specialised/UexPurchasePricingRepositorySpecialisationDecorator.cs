namespace Arkanis.Overlay.Infrastructure.Repositories.Local.Specialised;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;

internal class UexPurchasePricingRepositorySpecialisationDecorator(IGameEntityRepository<GameEntityPurchasePrice> repository)
    : UexPricingRepositorySpecialisationDecorator<GameEntityPurchasePrice, IGameEntityPurchasePrice>(repository), IGamePurchasePricingRepository
{
    public IAsyncEnumerable<IGameEntityPurchasePrice> GetAllPurchasePricesAsync(CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAllAsync(cancellationToken);

    public async ValueTask<ICollection<IGameEntityPurchasePrice>> GetPurchasePricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default)
        => await GetAllForOwnerAsync(domainId, cancellationToken);
}
