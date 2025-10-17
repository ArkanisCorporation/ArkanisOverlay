namespace Arkanis.Overlay.External.UEX;

using Overlay.Common;
using Overlay.Common.Models;

public static class UexConstants
{
    public const string WebBaseUrl = "https://uexcorp.space";
    public const string ApiBaseUrl = "https://uex-proxy.arkanis.space/2.0";

    public const string AssetsBaseUrl = "https://assets.uexcorp.space/img";
    public const string AssetsPreferredUrl = "https://d3a1t0pe7zmm0w.cloudfront.net";

    public static readonly ExternalAuthenticatorInfo ProviderInfo = new()
    {
        ServiceId = ExternalService.UnitedExpress,
        DisplayName = "United Express (UEX)",
        Description
            = "United Express (UEX) plays the role of an interstellar trade and data provider corporation established in 2950."
              + " Headquartered in the vibrant city of New Babbage within the Stanton system, our corporation is dedicated to facilitating commerce and information exchange."
              + " Fueled by the passion of the Star Citizen community, UEX has rapidly become a premier source of up-to-date trade and mining data. "
              + " Their platform empowers players to discover the most lucrative profit opportunities in the game through meticulous analysis and insights.",
    };
}
