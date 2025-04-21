namespace Arkanis.Overlay.Infrastructure.Services.Abstractions;

using Domain.Abstractions.Game;

public interface IGameEntityHydratationService
{
    Task HydrateAsync<T>(T gameEntity) where T : IGameEntity;
}
