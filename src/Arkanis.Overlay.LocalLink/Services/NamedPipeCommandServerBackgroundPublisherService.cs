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
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (IOException e)
            {
                logger.LogError(e, "Could not start the LocalLink command server");
                break;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unhandled exception while receiving/publishing a LocalLink command");
                await Task.Delay(TimeSpan.FromSeconds(4), stoppingToken);
            }
        }
    }
}
