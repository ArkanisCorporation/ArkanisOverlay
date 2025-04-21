namespace Arkanis.Overlay.Infrastructure.Services.Hosted;

using Domain.Abstractions.Game;
using Microsoft.Extensions.Hosting;

internal class InitializeGameEntityRepositoryHostedService<T>(GameEntityRepositorySyncManager<T> repositorySyncManager)
    : BackgroundService where T : class, IGameEntity
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        => await repositorySyncManager.UpdateIfNecessaryAsync(stoppingToken);
}
