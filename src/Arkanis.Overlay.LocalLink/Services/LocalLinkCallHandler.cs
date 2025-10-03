namespace Arkanis.Overlay.LocalLink.Services;

using Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class LocalLinkCallHandler(
    IConfiguration configuration,
    CustomProtocolClient customProtocolClient,
    ILocalLinkCommandPublisher commandPublisher,
    ILogger<LocalLinkCallHandler> logger
) : LocalLinkCallHandlerBase(configuration, customProtocolClient, logger)
{
    protected override async Task<bool> TryProcessCommandEndpointCall(CommandLocalLinkEndpointCall endpointCall, CancellationToken cancellationToken)
    {
        try
        {
            var command = endpointCall.Command;
            logger.LogInformation("Processing command endpoint call: {@Command}", command);
            await commandPublisher.PublishAsync(command, cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error was encountered while processing a command endpoint call");
            return false;
        }
    }

    public class HostedService(LocalLinkCallHandler handler) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            => await handler.TryProcessCustomProtocolCallFromConfigurationAsync(stoppingToken);
    }
}
