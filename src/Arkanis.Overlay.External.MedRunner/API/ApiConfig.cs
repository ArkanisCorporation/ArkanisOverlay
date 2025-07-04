namespace Arkanis.Overlay.External.MedRunner.API;

/// <summary>
///     Configuration for the Medrunner API.
/// </summary>
public class ApiConfig
{
    /// <summary>
    ///     The base URL of the API - defaults to https://api.medrunner.space
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.medrunner.space";

    /// <summary>
    ///     Your API token retrieved after logging in. If none is provided, the refresh token will be used to retrieve an
    ///     access token.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    ///     Your refresh token, used to obtain new API tokens. If none is provided, authenticated requests will not be possible
    ///     when the access token expires.
    ///     If no access token is provided either, only unauthenticated requests are possible.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    ///     Use cookie based auth instead of tokens - defaults to false
    /// </summary>
    public bool CookieAuth { get; set; }
}
