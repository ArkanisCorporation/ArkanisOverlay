namespace Arkanis.Overlay.External.MedRunner.API;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Models;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

public sealed class ApiKeySourcedTokenProvider(
    IServiceProvider serviceProvider,
    IMedRunnerClientConfig config,
    IMemoryCache memoryCache,
    ILogger<ApiKeySourcedTokenProvider> logger
) : IMedRunnerTokenProvider, IDisposable
{
    private readonly SemaphoreSlim _accessTokenRequestSemaphore = new(1, 1);
    private readonly JsonWebTokenHandler _tokenHandler = new();

    private readonly TokenValidationParameters _tokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidIssuers = ["medrunner.space"],
        ValidateAudience = false,
        ValidAudiences = [],
        ValidateIssuerSigningKey = false,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        RequireSignedTokens = false,
        //? custom validator prevents token signature validation
        //? however, it also bypasses default inbound claim mappings
        SignatureValidator = (token, _) => new JsonWebToken(token),
        NameClaimType = JwtRegisteredClaimNames.UniqueName,
        RoleClaimType = "role",
        ClockSkew = TimeSpan.FromSeconds(2),
    };

    private IMedRunnerApiClient? _apiClient;

    private IMedRunnerApiClient ApiClient
        => _apiClient ??= serviceProvider.GetRequiredService<IMedRunnerApiClient>();

    public void Dispose()
        => _accessTokenRequestSemaphore.Dispose();

    public ClaimsIdentity? Identity { get; private set; }

    [MemberNotNullWhen(true, nameof(Identity))]
    public bool IsAuthenticated
        => Identity is not null && config.AccessToken is not null;

    public Task<string?> GetAccessTokenAsync()
        => GetAccessTokenAsync("unknown");

    public async Task<string?> GetAccessTokenAsync(string source)
    {
        // fast asynchronous token check
        if (await ValidateTokenAsync() is { Length: > 0 } accessToken)
        {
            return accessToken;
        }

        logger.LogDebug("Token validation unsuccessful, requesting a new token");
        await _accessTokenRequestSemaphore.WaitAsync();

        try
        {
            var cacheKey = $"{nameof(ApiKeySourcedTokenProvider)}-{nameof(GetAccessTokenAsync)}-{config.RefreshToken}";
            return await memoryCache.GetOrCreateAsync(
                cacheKey,
                async entry =>
                {
                    // synchronous token check (may have already been updated by a concurrent request)
                    if (await ValidateTokenAsync() is { Length: > 0 } newAccessToken)
                    {
                        return newAccessToken;
                    }

                    if (string.IsNullOrWhiteSpace(config.RefreshToken))
                    {
                        entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                        return null;
                    }

                    var result = await ApiClient.Auth.RequestTokenAsync(config.RefreshToken);
                    if (result.Success)
                    {
                        var expirationTime = result.Data.AccessTokenExpiration - DateTimeOffset.Now is { TotalSeconds: > 0 } timeSpan
                            ? timeSpan
                            : TimeSpan.FromSeconds(1);

                        entry.SetAbsoluteExpiration(expirationTime);
                        return await ValidateTokenAsync(result.Data);
                    }

                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                    logger.LogError("Failed to receive new access token: (status {StatusCode}) {Error}", result.StatusCode, result.ErrorMessage);
                    return null;
                }
            );
        }
        finally
        {
            _accessTokenRequestSemaphore.Release();
        }
    }

    private async Task<string?> ValidateTokenAsync(TokenGrant? tokenGrant = null)
    {
        var accessToken = config.AccessToken ?? tokenGrant?.AccessToken;
        var refreshToken = config.RefreshToken ?? tokenGrant?.RefreshToken;
        var validationResult = await _tokenHandler.ValidateTokenAsync(accessToken, _tokenValidationParameters);
        if (!validationResult.IsValid)
        {
            logger.LogWarning(validationResult.Exception, "Access token validation failed");
            return config.AccessToken = null;
        }

        config.AccessToken = accessToken;
        config.RefreshToken = refreshToken;
        Identity = validationResult.ClaimsIdentity;

        logger.LogDebug("Access token currently valid for {IdentityName}", Identity.Name);
        return accessToken;
    }
}
