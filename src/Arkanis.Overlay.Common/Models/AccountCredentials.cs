namespace Arkanis.Overlay.Common.Models;

using System.Text.Json.Serialization;

[JsonPolymorphic]
[JsonDerivedType(typeof(AccountEmptyCredentials), "v1/none")]
[JsonDerivedType(typeof(AccountOidcCredentials), "v1/oidc")]
[JsonDerivedType(typeof(AccountOAuth2Credentials), "v1/oauth2")]
[JsonDerivedType(typeof(AccountApiTokenCredentials), "v1/token")]
public record AccountCredentials([property: JsonRequired] string ServiceId);

public sealed record AccountEmptyCredentials(string ServiceId) : AccountCredentials(ServiceId);

public sealed record AccountApiTokenCredentials(string ServiceId) : AccountCredentials(ServiceId)
{
    public string? UserIdentifier { get; set; }
    public required string SecretToken { get; set; }
}

public record AccountOAuth2Credentials(string ServiceId) : AccountCredentials(ServiceId)
{
    public required string AccessToken { get; set; }
    public DateTimeOffset? AccessTokenExpiresAt { get; set; }
    public string? RefreshToken { get; set; }
}

public sealed record AccountOidcCredentials(string ServiceId) : AccountOAuth2Credentials(ServiceId)
{
    public required string IdToken { get; set; }
}
