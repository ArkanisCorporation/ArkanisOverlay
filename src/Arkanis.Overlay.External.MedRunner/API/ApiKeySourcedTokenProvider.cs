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

public class ApiKeySourcedTokenProvider(IServiceProvider serviceProvider, ApiConfig config, ILogger<ApiKeySourcedTokenProvider> logger)
    : IMedRunnerTokenProvider
{
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

    private TokenGrant? _tokenGrant;

    private IMedRunnerApiClient ApiClient
        => _apiClient ??= serviceProvider.GetRequiredService<IMedRunnerApiClient>();

    public ClaimsIdentity? Identity { get; private set; }

    [MemberNotNullWhen(true, nameof(Identity))]
    public bool IsAuthenticated
        => this is { _tokenGrant: not null, Identity: not null };

    public Task<string?> GetAccessTokenAsync()
        => GetAccessTokenAsync("unknown");

    public async Task<string?> GetAccessTokenAsync(string source)
    {
        if (await ValidateTokenAsync() is { Length: > 0 } accessToken)
        {
            return accessToken;
        }

        logger.LogDebug("Token validation unsuccessful, requesting a new token");
        var result = await ApiClient.Auth.RequestTokenAsync(config.RefreshToken ?? string.Empty);
        if (result.Success)
        {
            return await ValidateTokenAsync(result.Data);
        }

        logger.LogError("Failed to receive new access token: (status {StatusCode}) {Error}", result.StatusCode, result.ErrorMessage);
        return null;
    }

    private async Task<string?> ValidateTokenAsync(TokenGrant? tokenGrant = null)
    {
        tokenGrant ??= _tokenGrant;
        if (tokenGrant is null)
        {
            return null;
        }

        var accessToken = tokenGrant.AccessToken;
        var validationResult = await _tokenHandler.ValidateTokenAsync(accessToken, _tokenValidationParameters);
        if (!validationResult.IsValid)
        {
            logger.LogWarning(validationResult.Exception, "Access token validation failed");
            return null;
        }

        _tokenGrant = tokenGrant;
        Identity = validationResult.ClaimsIdentity;
        return accessToken;
    }
}
