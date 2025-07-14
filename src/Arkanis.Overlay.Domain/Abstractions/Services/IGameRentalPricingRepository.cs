namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Common.Abstractions.Services;
using Game;

public interface IGameRentalPricingRepository : IDependable
{
    IAsyncEnumerable<IGameEntityRentalPrice> GetAllRentalPricesAsync(CancellationToken cancellationToken = default);

    ValueTask<ICollection<IGameEntityRentalPrice>> GetRentalPricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default);
}
