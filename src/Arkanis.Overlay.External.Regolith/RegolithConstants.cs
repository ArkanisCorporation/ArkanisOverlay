namespace Arkanis.Overlay.External.Regolith;

using Overlay.Common;
using Overlay.Common.Models;

public static class RegolithConstants
{
    public const string WebBaseUrl = "https://regolith.rocks";
    public const string ApiBaseUrl = "https://api.regolith.rocks";
    public const string AccountSettingsUrl = "https://regolith.rocks/profile/settings";

    public static readonly ExternalAuthenticatorInfo ProviderInfo = new()
    {
        ServiceId = ExternalService.Regolith,
        DisplayName = "Regolith Co.",
        Description
            = "Regolith Co. is a fansite dedicated to helping Star Citizen Miners organize, share, and scout together. "
              + "The tool can capture work orders and rock scans, calculate mining loadouts, track refinery timers, "
              + "and provides collaborative mining session management. "
              + "It offers comprehensive survey data about ore probabilities and locations across the verse.",
    };
}
