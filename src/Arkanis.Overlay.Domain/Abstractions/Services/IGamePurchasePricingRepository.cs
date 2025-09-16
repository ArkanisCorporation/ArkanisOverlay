namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Common.Abstractions.Services;
using Game;

public interface IGamePurchasePricingRepository : IDependable
{
    IAsyncEnumerable<IGameEntityPurchasePrice> GetAllPurchasePricesAsync(CancellationToken cancellationToken = default);

    ValueTask<ICollection<IGameEntityPurchasePrice>> GetPurchasePricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default);
}
