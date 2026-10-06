namespace Arkanis.Overlay.Common.Services;

using System.Security.Claims;
using Models;

public abstract class ExternalAuthResultBase
{
    public abstract ExternalAuthenticatorInfo ProviderInfo { get; }

    public ClaimsIdentity Identity { get; protected set; } = new();

    public DateTimeOffset ExpiresAt { get; protected set; }

    public virtual bool IsAuthenticated
        => Identity.IsAuthenticated;
}
