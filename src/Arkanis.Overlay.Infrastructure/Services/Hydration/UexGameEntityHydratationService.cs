namespace Arkanis.Overlay.Infrastructure.Services.Hydration;

using Abstractions;
using Domain.Abstractions.Game;

/// <inheritdoc cref="IGameEntityHydrationService"/>
public class GameEntityHydrationService(IEnumerable<IHydrationServiceFor> hydrationServices) : IGameEntityHydrationService
{
    public async Task HydrateAsync<T>(T gameEntity, CancellationToken cancellationToken) where T : IGameEntity
    {
        foreach (var hydrationService in hydrationServices)
        {
            await hydrationService.HydrateAsync(gameEntity, cancellationToken);
        }
    }
}
