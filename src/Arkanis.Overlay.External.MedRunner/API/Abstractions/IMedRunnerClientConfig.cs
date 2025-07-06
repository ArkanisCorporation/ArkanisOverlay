namespace Arkanis.Overlay.External.MedRunner.API.Abstractions;

/// <summary>
///     Configuration for the MedRunner API client.
/// </summary>
public interface IMedRunnerClientConfig
{
    /// <summary>
    ///     The base URL of the API - defaults to https://api.medrunner.space
    /// </summary>
    string BaseUrl { get; internal set; }

    /// <summary>
    ///     Your API token retrieved after logging in.
    ///     If none is provided, the refresh token will be used to retrieve an access token.
    /// </summary>
    string? AccessToken { get; internal set; }

    /// <summary>
    ///     Your refresh token, used to obtain new API tokens.
    ///     If none is provided, authenticated requests will not be possible when the access token expires.
    ///     If no access token is provided either, only unauthenticated requests are possible.
    /// </summary>
    string? RefreshToken { get; internal set; }

    /// <summary>
    ///     Use cookie based auth instead of tokens - defaults to false
    /// </summary>
    bool CookieAuth { get; internal set; }

    /// <summary>
    ///     Use mock data instead of the real API - defaults to false
    /// </summary>
    public bool IsMock { get; internal set; }

    public void SetApiToken(string apiToken)
    {
        AccessToken = null;
        RefreshToken = apiToken;
    }
}
