namespace Arkanis.Overlay.LocalLink.Services;

using Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class NamedPipeCommandServerBackgroundPublisherService(
    NamedPipeCommandServer commandServer,
    ILocalLinkCommandPublisher commandPublisher,
    ILogger<NamedPipeCommandServerBackgroundPublisherService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var command = await commandServer.ReceiveAsync(TimeSpan.FromSeconds(5), stoppingToken);
                await commandPublisher.PublishAsync(command, stoppingToken);
            }
            catch (IOException e)
            {
                logger.LogError(e, "Could not start the LocalLink command server");
                break;
            }
        }
    }
}
