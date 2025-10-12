namespace Arkanis.Overlay.External.Backend.Options;

using Common.Abstractions;

public class ArkanisBackendOptions : ISelfBindableOptions
{
    public required string HttpClientBaseAddress { get; set; } = "https://api.arkanis.cc/graphql";
    public required string WebSocketClientBaseAddress { get; set; } = "wss://api.arkanis.cc/graphql";

    public string SectionPath
        => "ArkanisBackend";
}
