namespace Arkanis.Overlay.LocalLink.Services;

using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public abstract class LocalLinkCallHandlerBase(
    IConfiguration configuration,
    CustomProtocolClient customProtocolClient,
    ILogger logger
)
{
    public async Task<bool> TryProcessCustomProtocolCallFromConfigurationAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Trying to process custom protocol invocation");
        var customProtocolContentKey = ApplicationConstants.Args.Config.GetKeyFor(ApplicationConstants.Args.HandleUrl);
        if (configuration[customProtocolContentKey] is not { Length: > 0 } customProtocolContent)
        {
            logger.LogInformation("No custom protocol content found: {ConfigurationKey}", customProtocolContentKey);
            return false;
        }

        return await TryProcessCustomProtocolCallAsync(customProtocolContent, cancellationToken);
    }

    public async Task<bool> TryProcessCustomProtocolCallAsync(string customProtocolUrl, CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing custom protocol content: {Content}", customProtocolUrl);
        var endpointCall = customProtocolClient.ParseEndpointCall(customProtocolUrl);
        if (endpointCall is CommandLocalLinkEndpointCall commandEndpointCall)
        {
            return await TryProcessCommandEndpointCall(commandEndpointCall, cancellationToken);
        }

        logger.LogError("Unsupported local link endpoint call: {@EndpointCall}", endpointCall);
        return false;
    }

    protected abstract Task<bool> TryProcessCommandEndpointCall(CommandLocalLinkEndpointCall endpointCall, CancellationToken cancellationToken);
}
