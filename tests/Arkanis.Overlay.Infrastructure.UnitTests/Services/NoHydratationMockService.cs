namespace Arkanis.Overlay.Infrastructure.UnitTests.Services;

using Domain.Abstractions.Game;
using Infrastructure.Services.Abstractions;

public class NoHydratationMockService : IGameEntityHydratationService
{
    public Task HydrateAsync<T>(T gameEntity) where T : IGameEntity
        => Task.CompletedTask;
}
