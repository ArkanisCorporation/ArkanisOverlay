namespace Arkanis.Overlay.Infrastructure.Services.Hosted;

using Domain.Abstractions.Game;
using Quartz;

internal class GameEntityRepositoryUpdateIfNecessaryJob<T>(GameEntityRepositorySyncManager<T> repositorySyncManager)
    : IJob where T : class, IGameEntity
{
    public async Task Execute(IJobExecutionContext context)
        => await repositorySyncManager.UpdateIfNecessaryAsync(context.CancellationToken);
}
