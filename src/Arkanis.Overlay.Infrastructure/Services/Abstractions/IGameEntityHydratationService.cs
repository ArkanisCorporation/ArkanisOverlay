namespace Arkanis.Overlay.Infrastructure.Services.Abstractions;

using Domain.Abstractions.Game;

/// <summary>
///     Aggregates all type-specific hydration services into a single service.
/// </summary>
public interface IGameEntityHydrationService
{
    Task HydrateAsync<T>(T gameEntity, CancellationToken cancellationToken = default) where T : IGameEntity;
}
