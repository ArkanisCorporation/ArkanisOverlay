namespace Arkanis.Overlay.Infrastructure.Services.Hosted;

using Domain.Abstractions.Game;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Quartz;

internal class GameEntityRepositorySyncManagerUpdateIfNecessaryJob<T>(
    GameEntityRepositorySyncManager<T> repositorySyncManager,
    ILogger<GameEntityRepositorySyncManagerUpdateIfNecessaryJob<T>> logger
) : IJob where T : class, IGameEntity
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogDebug("Running scheduled update (if necessary) for {GameEntityType}", typeof(T).ShortDisplayName());
        await repositorySyncManager.UpdateIfNecessaryAsync(context.CancellationToken);
    }
}
