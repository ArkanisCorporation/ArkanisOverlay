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
        await Task.WhenAll(services.Select(service => service.InitializeAsync(stoppingToken))).ConfigureAwait(false);
    }
}
