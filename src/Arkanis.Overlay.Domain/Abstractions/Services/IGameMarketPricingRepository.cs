namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models.Game;

public interface IGameMarketPricingRepository : IDependable
{
    public IAsyncEnumerable<GameEntityMarketPrice> GetAllMarketPricesAsync(CancellationToken cancellationToken = default);

    public ValueTask<ICollection<GameEntityMarketPrice>> GetMarketPricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default);
}
