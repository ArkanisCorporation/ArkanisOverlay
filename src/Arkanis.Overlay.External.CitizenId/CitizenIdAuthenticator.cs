namespace Arkanis.Overlay.External.CitizenId;

using Common;
using Common.Models;
using Common.Services;
using Duende.IdentityModel.OidcClient;

public class CitizenIdAuthenticator(IServiceProvider serviceProvider) : OidcAuthenticator(serviceProvider)
{
    public override ExternalAuthenticatorInfo AuthenticatorInfo
        => CitizenIdConstants.ProviderInfo;

    public override OidcClient OidcClient
        => new(OidcOptions);

    private static OidcClientOptions OidcOptions { get; } = new()
    {
        Authority = CitizenIdConstants.Authority,
        ClientId = "arkaniscorp",
        Scope = "openid profile rsi.profile",
        RedirectUri = $"{ApplicationConstants.Protocol.Schema}://citizenid.space/sign-in",
    };
}
