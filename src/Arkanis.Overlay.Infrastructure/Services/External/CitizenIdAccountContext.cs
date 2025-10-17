namespace Arkanis.Overlay.Infrastructure.Services.External;

using System.Security.Claims;
using Common.Abstractions;
using Common.Models;
using Common.Services;
using Microsoft.Extensions.Logging;
using Overlay.External.CitizenId;

public class CitizenIdAccountContext(
    CitizenIdAuthenticator authenticator,
    CitizenIdLinkHelper linkHelper,
    IUserPreferencesManager userPreferences,
    ILogger<UexAccountContext> logger
)
    : ExternalAccountContext<OidcAuthenticator.AuthenticationTask>(authenticator, userPreferences, logger)
{
    private const string RsiUsernameClaim = "urn:user:rsi:username";
    private const string RsiAvatarClaim = "urn:user:rsi:avatar:url";

    public CitizenIdLinkHelper Links { get; } = linkHelper;

    public ClaimsIdentity RsiIdentity { get; private set; } = new();

    protected override Task UpdateAsyncCore(CancellationToken cancellationToken)
    {
        if (!IsAuthenticated || Identity.FindFirst(RsiUsernameClaim) is not { } rsiUsernameClaim)
        {
            RsiIdentity = new ClaimsIdentity();
            return Task.CompletedTask;
        }


        var claims = new List<Claim>
        {
            rsiUsernameClaim,
            new(AccountClaimTypes.DisplayName, rsiUsernameClaim.Value),
        };
        if (Identity.FindFirst(RsiAvatarClaim) is { } rsiAvatarClaim)
        {
            claims.Add(rsiAvatarClaim);
            claims.Add(new Claim(AccountClaimTypes.AvatarUrl, rsiAvatarClaim.Value));
        }

        RsiIdentity = new ClaimsIdentity(claims, claims.Count != 0 ? AuthenticatorInfo.ServiceId : null, AccountClaimTypes.DisplayName, null);
        return Task.CompletedTask;
    }
}
