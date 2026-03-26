namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;

public interface IGamePurchasePricingRepository : IDependable
{
    public IAsyncEnumerable<IGameEntityPurchasePrice> GetAllPurchasePricesAsync(CancellationToken cancellationToken = default);

    public ValueTask<ICollection<IGameEntityPurchasePrice>> GetPurchasePricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default);
}
