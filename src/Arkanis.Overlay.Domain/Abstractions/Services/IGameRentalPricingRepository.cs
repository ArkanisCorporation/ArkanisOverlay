namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;

public interface IGameRentalPricingRepository : IDependable
{
    public IAsyncEnumerable<IGameEntityRentalPrice> GetAllRentalPricesAsync(CancellationToken cancellationToken = default);

    public ValueTask<ICollection<IGameEntityRentalPrice>> GetRentalPricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default);
}
