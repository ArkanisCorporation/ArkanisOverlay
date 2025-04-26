namespace Arkanis.Overlay.Infrastructure.Services.Hydration;

using Abstractions;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;

/// <summary>
///     Hydrates any <see cref="IGameSellable" /> entity with all appropriate data points.
/// </summary>
/// <param name="salePriceProvider">A sale price provider</param>
public class UexGameSellableHydrationService(ISalePriceProvider salePriceProvider) : IHydrationServiceFor<IGameSellable>
{
    public async Task HydrateAsync(IGameSellable entity, CancellationToken cancellationToken = default)
    {
        await WaitUntilReadyAsync(cancellationToken);

        await salePriceProvider.UpdatePriceTagAsync(entity);
    }

    public bool IsReady
        => salePriceProvider.IsReady;

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await salePriceProvider.WaitUntilReadyAsync(cancellationToken);
}
