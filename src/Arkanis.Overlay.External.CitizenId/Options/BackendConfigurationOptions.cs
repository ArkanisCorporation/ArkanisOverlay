namespace Arkanis.Overlay.External.CitizenId.Options;

using global::CitizenId.Domain.Shared;
using global::CitizenId.Domain.Shared.Authorization;
using Overlay.Common.Abstractions;

public class CitizenIdOptions : ISelfBindableOptions
{
    public Uri BaseAddress { get; set; } = CitizenIdDeployments.Production.Url;
    public Uri Authority { get; set; } = CitizenIdDeployments.Production.Url;
    public string ClientId { get; set; } = "arkaniscorp-overlay";

    public string[] Scopes { get; set; } =
    [
        "openid",
        CitizenIdScopes.Profile,
        CitizenIdScopes.RsiProfile,
        CitizenIdScopes.DiscordProfile,
        CitizenIdScopes.OfflineAccess,
    ];

    public string SectionPath
        => "CitizenId";
}
