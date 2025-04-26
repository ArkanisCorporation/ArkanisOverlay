namespace Arkanis.Overlay.Infrastructure.Services.Hydration;

using Abstractions;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;

/// <summary>
///     Hydrates any <see cref="IGameRentable" /> entity with all appropriate data points.
/// </summary>
/// <param name="rentPriceProvider">A rental price provider</param>
public class UexGameRentableHydrationService(IRentPriceProvider rentPriceProvider) : IHydrationServiceFor<IGameRentable>
{
    public async Task HydrateAsync(IGameRentable entity, CancellationToken cancellationToken = default)
    {
        await WaitUntilReadyAsync(cancellationToken);

        await rentPriceProvider.UpdatePriceTagAsync(entity);
    }

    public bool IsReady
        => rentPriceProvider.IsReady;

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => await rentPriceProvider.WaitUntilReadyAsync(cancellationToken);
}
