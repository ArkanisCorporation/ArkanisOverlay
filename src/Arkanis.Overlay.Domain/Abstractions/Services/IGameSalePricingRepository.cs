namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;

public interface IGameSalePricingRepository : IDependable
{
    public IAsyncEnumerable<IGameEntitySalePrice> GetAllSalePricesAsync(CancellationToken cancellationToken = default);

    public ValueTask<ICollection<IGameEntitySalePrice>> GetSalePricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default);
}
