namespace Arkanis.Overlay.Infrastructure.Repositories.Local.Specialised;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;

internal class UexSalePricingRepositorySpecialisationDecorator(IGameEntityRepository<GameEntitySalePrice> repository)
    : UexPricingRepositorySpecialisationDecorator<GameEntitySalePrice, IGameEntitySalePrice>(repository), IGameSalePricingRepository
{
    public IAsyncEnumerable<IGameEntitySalePrice> GetAllSalePricesAsync(CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAllAsync(cancellationToken);

    public async ValueTask<ICollection<IGameEntitySalePrice>> GetSalePricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default)
        => await GetAllForOwnerAsync(domainId, cancellationToken);
}
