namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Auth;

using Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;
using Request;

/// <summary>
///     Represents the authentication endpoint.
/// </summary>
public class AuthEndpoint(ApiConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger)
{
    protected override string Endpoint
        => "auth";

    /// <summary>
    ///     Invalidate a refresh token.
    /// </summary>
    /// <param name="oldToken">Token to be invalidated</param>
    public async Task<ApiResponse<string>> SignOutAsync(SignOutRequest? oldToken = null)
        => await PostRequestAsync<string>("/signOut", oldToken);

    /// <summary>
    ///     Gets all API tokens for the user.
    /// </summary>
    public async Task<ApiResponse<List<ApiToken>>> GetApiTokensAsync()
        => await GetRequestAsync<List<ApiToken>>("/apiTokens");

    /// <summary>
    ///     Creates an API token.
    /// </summary>
    /// <param name="newToken">Details for the new API token</param>
    /// <returns>The newly-created API token</returns>
    public async Task<ApiResponse<string>> CreateApiTokenAsync(CreateApiTokenRequest newToken)
    {
        var payload = new
        {
            name = newToken.Name,
            expirationDate = newToken.ExpirationDate?.ToString("o"),
        };
        return await PostRequestAsync<string>("/apiTokens", payload);
    }

    /// <summary>
    ///     Delete an API token.
    /// </summary>
    /// <param name="id">The id of the API token to delete</param>
    public async Task<ApiResponse<string>> DeleteApiTokenAsync(string id)
        => await DeleteRequestAsync<string>($"/apiTokens/{id}");

    public async Task<ApiResponse<TokenGrant>> RequestTokenAsync(string refreshToken)
    {
        var body = new { refreshToken };
        return await PostRequestAsync<TokenGrant>("/exchange", body, true);
    }
}
