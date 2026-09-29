namespace Arkanis.Overlay.External.MedRunner.API;

using System.Security.Claims;

public sealed record MedRunnerTokenAuthentication
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required ClaimsIdentity Identity { get; init; }
}
