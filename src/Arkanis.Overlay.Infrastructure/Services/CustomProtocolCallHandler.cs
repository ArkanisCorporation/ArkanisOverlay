namespace Arkanis.Overlay.Infrastructure.Services;

using LocalLink.Abstractions;
using LocalLink.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class CustomProtocolCallHandler(
    IConfiguration configuration,
    CustomProtocolClient customProtocolClient,
    ILocalLinkCommandPublisher commandPublisher,
    ILogger<CustomProtocolCallHandler> logger
) : CustomProtocolCallHandlerBase(configuration, customProtocolClient, logger)
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

    public class HostedService(CustomProtocolCallHandler handler) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
            => await handler.TryProcessCustomProtocolCallFromConfigurationAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
