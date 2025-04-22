namespace Arkanis.Overlay.Infrastructure.Services.Hosted;

using Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class InitializeServicesHostedService(
    IEnumerable<ISelfInitializable> services,
    ILogger<InitializeServicesHostedService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Running initialization of services: {@Services}", services);
        var tasks = services.Select(async service =>
            {
                logger.LogDebug("Starting initialization: {ServiceType}", service);
                await service.InitializeAsync(stoppingToken);
                logger.LogDebug("Successfully initialized: {ServiceType}", service);
            }
        );

        await Task.WhenAll(tasks).ConfigureAwait(false);
        logger.LogDebug("Finished initialization of {ServiceCount} services", services.Count());
    }
}
