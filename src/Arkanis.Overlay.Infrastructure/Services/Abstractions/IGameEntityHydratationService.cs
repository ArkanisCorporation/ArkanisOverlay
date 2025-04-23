namespace Arkanis.Overlay.Infrastructure.Services.Abstractions;

using Domain.Abstractions;
using Domain.Abstractions.Game;

public interface IGameEntityHydrationService : IDependable
{
    Task HydrateAsync<T>(T gameEntity, CancellationToken cancellationToken = default) where T : IGameEntity;
}
