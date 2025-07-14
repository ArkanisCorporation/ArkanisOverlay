namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Auth.Request;

/// <summary>
///     Request body for sign-out.
/// </summary>
public class SignOutRequest
{
    /// <summary>
    ///     The refresh token to be invalidated
    /// </summary>
    public required string RefreshToken { get; set; }
}
