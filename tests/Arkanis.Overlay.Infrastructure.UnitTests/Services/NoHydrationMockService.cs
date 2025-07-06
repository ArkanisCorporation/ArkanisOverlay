namespace Arkanis.Overlay.Infrastructure.UnitTests.Services;

using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions.Game;
using Infrastructure.Services.Abstractions;

public class NoHydrationMockService : IGameEntityHydrationService
{
    public Task HydrateAsync<T>(T gameEntity, CancellationToken cancellationToken = default) where T : IGameEntity
        => Task.CompletedTask;
}
