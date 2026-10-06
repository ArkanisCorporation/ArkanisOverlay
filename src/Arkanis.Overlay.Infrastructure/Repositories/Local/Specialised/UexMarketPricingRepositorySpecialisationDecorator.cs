namespace Arkanis.Overlay.Infrastructure.Repositories.Local.Specialised;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;

internal class UexMarketPricingRepositorySpecialisationDecorator(IGameEntityRepository<GameEntityMarketPrice> repository)
    : UexPricingRepositorySpecialisationDecorator<GameEntityMarketPrice>(repository), IGameMarketPricingRepository
{
    public IAsyncEnumerable<GameEntityMarketPrice> GetAllMarketPricesAsync(CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAllAsync(cancellationToken);

    public async ValueTask<ICollection<GameEntityMarketPrice>> GetMarketPricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default)
        => await GetAllForOwnerAsync(domainId, cancellationToken);
}
