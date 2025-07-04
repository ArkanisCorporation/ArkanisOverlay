namespace Arkanis.Overlay.External.MedRunner.Models;

/// <summary>
///     Represents an API token.
/// </summary>
public class ApiToken
{
    /// <summary>
    ///     The user who created the token.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    ///     Human-readable name for the token, assigned by the user.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     The timestamp at which the token will expire in Unix seconds.
    /// </summary>
    public long? ExpirationDate { get; set; }

    /// <summary>
    ///     When the token was last used to generate a new access token, iso-8601 timestamp.
    /// </summary>
    public string? LastUsed { get; set; }
}
