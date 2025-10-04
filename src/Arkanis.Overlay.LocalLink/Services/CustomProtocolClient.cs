namespace Arkanis.Overlay.LocalLink.Services;

using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using System.Web;
using Common;
using Microsoft.Extensions.Logging;
using Models;

public class CustomProtocolClient(ILogger<CustomProtocolClient> logger)
{
    public const string CommandEndpoint = "/command";
    public const string CommandEndpointDataParameter = "data";

    public Uri CreateUriFor(LocalLinkCommandBase command)
    {
        var commandData = JsonSerializer.Serialize(command, LocalLinkConstants.SerializerOptions);
        var base64CommandData = Convert.ToBase64String(Encoding.UTF8.GetBytes(commandData));
        return ApplicationConstants.Protocol.CreateUriFor($"{CommandEndpoint}?data={base64CommandData}");
    }

    public LocalLinkEndpointCall ParseEndpointCall(string uriPathAndQuery)
    {
        var uri = new Uri(uriPathAndQuery);

        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        return uri.AbsolutePath switch
        {
            CommandEndpoint => ParseCommandEndpointCall(queryParams),
            _ => new UnsupportedLocalLinkEndpointCall(uriPathAndQuery),
        };
    }

    private LocalLinkEndpointCall ParseCommandEndpointCall(NameValueCollection queryParams)
    {
        if (queryParams.GetValues(CommandEndpointDataParameter) is not { Length: 1 } values
            || values.First() is not { Length: > 0 } base64CommandData
            || string.IsNullOrWhiteSpace(base64CommandData))
        {
            logger.LogWarning("Invalid command endpoint call, the {QueryParamName} query parameter is missing or is empty", CommandEndpointDataParameter);
            return new MalformedLocalLinkEndpointCall(CommandEndpoint, queryParams);
        }

        try
        {
            var commandJson = Convert.FromBase64String(base64CommandData);
            var command = JsonSerializer.Deserialize<LocalLinkCommandBase>(commandJson, LocalLinkConstants.SerializerOptions);
            if (command is null)
            {
                logger.LogWarning("Invalid command endpoint call, could not deserialize the command from: {CommandJsonData}", commandJson);
                return new MalformedLocalLinkEndpointCall(CommandEndpoint, queryParams);
            }

            return new CommandLocalLinkEndpointCall(command);
        }
        catch (Exception e)
        {
            return new MalformedLocalLinkEndpointCall(CommandEndpoint, queryParams)
            {
                Exception = e,
            };
        }
    }
}
