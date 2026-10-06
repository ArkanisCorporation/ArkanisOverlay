namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Common.Abstractions.Services;
using Game;
using Models.Game;

public interface IGameMarketPricingRepository : IDependable
{
    IAsyncEnumerable<GameEntityMarketPrice> GetAllMarketPricesAsync(CancellationToken cancellationToken = default);

    ValueTask<ICollection<GameEntityMarketPrice>> GetMarketPricesForAsync(IDomainId domainId, CancellationToken cancellationToken = default);
}
