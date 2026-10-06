namespace Arkanis.Overlay.LocalLink.Services;

using System.Collections.Specialized;
using Models;

public abstract record LocalLinkEndpointCall;

public sealed record CommandLocalLinkEndpointCall(LocalLinkCommandBase Command) : LocalLinkEndpointCall;

public sealed record UnsupportedLocalLinkEndpointCall(string PathAndQuery) : LocalLinkEndpointCall
{
    public static UnsupportedLocalLinkEndpointCall Create(string pathAndQuery)
        => new(pathAndQuery);
}

public sealed record MalformedLocalLinkEndpointCall(string EndpointName, NameValueCollection QueryParams) : LocalLinkEndpointCall
{
    public Exception? Exception { get; init; }
}
