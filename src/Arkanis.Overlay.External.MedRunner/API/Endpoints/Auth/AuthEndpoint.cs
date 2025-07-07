namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Auth;

using Abstractions;
using Abstractions.Endpoints;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;
using Request;

/// <inheritdoc cref="IAuthEndpoint" />
public class AuthEndpoint(IMedRunnerClientConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger<AuthEndpoint> logger)
    : ApiEndpoint(config, tokenProvider, cache, logger), IAuthEndpoint
{
    /// <inheritdoc />
    protected override string Endpoint
        => "auth";

    /// <inheritdoc />
    public async Task<ApiResponse<string>> SignOutAsync(SignOutRequest? oldToken = null)
        => await PostRequestAsync<string>("/signOut", oldToken);

    /// <inheritdoc />
    public async Task<ApiResponse<List<ApiToken>>> GetApiTokensAsync()
        => await GetRequestAsync<List<ApiToken>>("/apiTokens");

    /// <inheritdoc />
    public async Task<ApiResponse<string>> CreateApiTokenAsync(CreateApiTokenRequest newToken)
    {
        var payload = new
        {
            name = newToken.Name,
            expirationDate = newToken.ExpirationDate?.ToString("o"),
        };
        return await PostRequestAsync<string>("/apiTokens", payload);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<string>> DeleteApiTokenAsync(string id)
        => await DeleteRequestAsync<string>($"/apiTokens/{id}");

    /// <inheritdoc />
    public async Task<ApiResponse<TokenGrant>> RequestTokenAsync(string refreshToken)
    {
        var body = new { refreshToken };
        return await PostRequestAsync<TokenGrant>("/exchange", body, RequestOptions.Unauthenticated);
    }
}
