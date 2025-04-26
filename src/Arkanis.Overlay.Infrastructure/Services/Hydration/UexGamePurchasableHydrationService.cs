namespace Arkanis.Overlay.Infrastructure.Services.Hydration;

using Abstractions;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;

/// <summary>
///     Hydrates any <see cref="IGamePurchasable" /> entity with all appropriate data points.
/// </summary>
/// <param name="purchasePriceProvider">A purchase price provider</param>
public class UexGamePurchasableHydrationService(IPurchasePriceProvider purchasePriceProvider) : IHydrationServiceFor<IGamePurchasable>
{
    public async Task HydrateAsync(IGamePurchasable entity, CancellationToken cancellationToken = default)
    {
        await WaitUntilReadyAsync(cancellationToken);

        await purchasePriceProvider.UpdatePriceTagAsync(entity);
    }

    public bool IsReady
        => purchasePriceProvider.IsReady;

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await purchasePriceProvider.WaitUntilReadyAsync(cancellationToken);
}
