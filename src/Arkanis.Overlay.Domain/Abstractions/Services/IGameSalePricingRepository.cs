namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Common.Abstractions.Services;
using Game;

public interface IGameSalePricingRepository : IDependable
{
    IAsyncEnumerable<IGameEntitySalePrice> GetAllSalePricesAsync(CancellationToken cancellationToken = default);

    ValueTask<ICollection<IGameEntitySalePrice>> GetSalePricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default);
}
