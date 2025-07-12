namespace Arkanis.Overlay.External.MedRunner.API.Abstractions.Endpoints;

using API.Endpoints.Auth.Request;
using Models;

/// <summary>
///     Represents the authentication endpoint.
/// </summary>
public interface IAuthEndpoint
{
    /// <summary>
    ///     Invalidate a refresh token.
    /// </summary>
    /// <param name="oldToken">Token to be invalidated (optional).</param>
    /// <returns>An API response indicating success or failure.</returns>
    Task<ApiResponse<string>> SignOutAsync(SignOutRequest? oldToken = null);

    /// <summary>
    ///     Gets all API tokens for the user.
    /// </summary>
    /// <returns>An API response containing a list of API tokens.</returns>
    Task<ApiResponse<List<ApiToken>>> GetApiTokensAsync();

    /// <summary>
    ///     Creates an API token.
    /// </summary>
    /// <param name="newToken">Details for the new API token.</param>
    /// <returns>An API response containing the newly-created API token.</returns>
    Task<ApiResponse<string>> CreateApiTokenAsync(CreateApiTokenRequest newToken);

    /// <summary>
    ///     Delete an API token.
    /// </summary>
    /// <param name="id">The ID of the API token to delete.</param>
    /// <returns>An API response indicating success or failure.</returns>
    Task<ApiResponse<string>> DeleteApiTokenAsync(string id);

    /// <summary>
    ///     Requests a new access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to exchange for a new access token.</param>
    /// <returns>An API response containing the new token grant.</returns>
    Task<ApiResponse<TokenGrant>> RequestTokenAsync(string refreshToken);
}
