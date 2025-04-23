namespace Arkanis.Overlay.Infrastructure.UnitTests.Services;

using Domain.Abstractions.Game;
using Infrastructure.Services.Abstractions;

public class NoHydrationMockService : IGameEntityHydrationService
{
    public Task HydrateAsync<T>(T gameEntity, CancellationToken cancellationToken = default) where T : IGameEntity
        => Task.CompletedTask;

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
