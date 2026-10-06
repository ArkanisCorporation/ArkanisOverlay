namespace Arkanis.Overlay.Infrastructure.Repositories.Local.Specialised;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;

internal class UexRentalPricingRepositorySpecialisationDecorator(IGameEntityRepository<GameEntityRentalPrice> repository)
    : UexPricingRepositorySpecialisationDecorator<GameEntityRentalPrice, IGameEntityRentalPrice>(repository), IGameRentalPricingRepository
{
    public IAsyncEnumerable<IGameEntityRentalPrice> GetAllRentalPricesAsync(CancellationToken cancellationToken = default)
        => DecoratedRepository.GetAllAsync(cancellationToken);

    public async ValueTask<ICollection<IGameEntityRentalPrice>> GetRentalPricesForAsync(IDomainId domainId, CancellationToken cancellationToken)
        => await GetAllForOwnerAsync(domainId, cancellationToken);
}
