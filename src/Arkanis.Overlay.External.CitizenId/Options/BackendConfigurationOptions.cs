namespace Arkanis.Overlay.External.CitizenId.Options;

using Common.Abstractions;

public class CitizenIdOptions : ISelfBindableOptions
{
    public Uri BaseAddress { get; set; } = new("https://citizenid.space");
    public Uri Authority { get; set; } = new("https://citizenid.space");
    public string ClientId { get; set; } = "arkaniscorp";
    public string[] Scopes { get; set; } = ["openid", "profile", "rsi.profile"];

    public string SectionPath
        => "CitizenId";
}
