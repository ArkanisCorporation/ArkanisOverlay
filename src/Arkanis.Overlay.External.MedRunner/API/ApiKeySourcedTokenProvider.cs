namespace Arkanis.Overlay.External.MedRunner.API;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Models;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

public sealed class ApiKeySourcedTokenProvider(IServiceProvider serviceProvider, IMedRunnerClientConfig config, ILogger<ApiKeySourcedTokenProvider> logger)
    : IMedRunnerTokenProvider, IDisposable
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
            // synchronous token check (may have already been updated by a concurrent request)
            if (await ValidateTokenAsync() is { Length: > 0 } newAccessToken)
            {
                return newAccessToken;
            }

            var result = await ApiClient.Auth.RequestTokenAsync(config.RefreshToken ?? string.Empty);
            if (result.Success)
            {
                return await ValidateTokenAsync(result.Data);
            }

            logger.LogError("Failed to receive new access token: (status {StatusCode}) {Error}", result.StatusCode, result.ErrorMessage);
            return null;
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
