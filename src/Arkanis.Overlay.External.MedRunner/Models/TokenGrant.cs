namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents a token grant.
/// </summary>
public class TokenGrant
{
    /// <summary>
    ///     The access token.
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    ///     The refresh token.
    /// </summary>
    public required string RefreshToken { get; set; }

    /// <summary>
    ///     The expiration date of the access token (ISO 8601 string).
    /// </summary>
    public required DateTimeOffset AccessTokenExpiration { get; set; }

    /// <summary>
    ///     The expiration date of the refresh token (ISO 8601 string, optional).
    /// </summary>
    public string? RefreshTokenExpiration { get; set; }
}
