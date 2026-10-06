namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Auth.Request;

/// <summary>
///     Request body for creating an API token.
/// </summary>
public class CreateApiTokenRequest
{
    /// <summary>
    ///     Human-readable name for the token
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Optional expiration date for the token
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
}
