namespace Arkanis.Overlay.External.CitizenId;

using Common;
using Common.Models;

public static class CitizenIdConstants
{
    public const string WebBaseUrl = "https://citizenid.space";
    public const string LinkAccountUrl = "https://api.arkanis.cc/api/v1/overlay/connect/link-account";
    public const string Authority = WebBaseUrl;

    public static readonly ExternalAuthenticatorInfo ProviderInfo = new()
    {
        ServiceId = ExternalService.CitizenId,
        DisplayName = "Citizen iD",
        Description
            = "Citizen iD is a community-driven identity provider for Star Citizen offering a secure and convenient way for players to authenticate and manage their game accounts across community projects.",
    };
}
