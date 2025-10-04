namespace Arkanis.Overlay.LocalLink.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class NamedPipeCommandCallForwarder(
    IConfiguration configuration,
    CustomProtocolClient customProtocolClient,
    NamedPipeCommandClient namedPipeCommandClient,
    ILogger<NamedPipeCommandCallForwarder> logger
) : LocalLinkCallHandlerBase(configuration, customProtocolClient, logger)
{
    protected override async Task<bool> TryProcessCommandEndpointCall(CommandLocalLinkEndpointCall endpointCall, CancellationToken cancellationToken)
    {
        try
        {
            var command = endpointCall.Command;
            logger.LogInformation("Forwarding command endpoint call: {@Command}", command);
            await namedPipeCommandClient.SendAsync(command, cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error was encountered while processing a command endpoint call");
            return false;
        }
    }
}
