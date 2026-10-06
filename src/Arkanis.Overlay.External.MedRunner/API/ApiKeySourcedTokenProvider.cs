namespace Arkanis.Overlay.External.MedRunner.API;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Abstractions;
using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
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
        if (await GetActiveAuthenticationAsync() is { } authentication)
        {
            return authentication.AccessToken;
        }

        logger.LogDebug("Token validation unsuccessful, requesting a new token for {Source}", source);
        await _accessTokenRequestSemaphore.WaitAsync();
        try
        {
            if (await GetActiveAuthenticationAsync() is { } currentAuthentication)
            {
                return currentAuthentication.AccessToken;
            }

            var refreshToken = config.RefreshToken;
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            var cacheKey = $"{nameof(ApiKeySourcedTokenProvider)}-{nameof(GetAccessTokenAsync)}-{refreshToken}";
            return await memoryCache.GetOrCreateAsync(
                cacheKey,
                async entry =>
                {
                    var authenticationResult = await AuthenticateApiTokenAsync(refreshToken, CancellationToken.None);
                    if (authenticationResult.IsSuccess)
                    {
                        var refreshedAuthentication = authenticationResult.Value;
                        entry.SetAbsoluteExpiration(
                            refreshedAuthentication.ExpiresAt > DateTimeOffset.Now
                                ? refreshedAuthentication.ExpiresAt
                                : DateTimeOffset.Now.AddSeconds(1)
                        );
                        ApplyAuthentication(refreshedAuthentication);
                        return refreshedAuthentication.AccessToken;
                    }

                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                    logger.LogError(
                        "Failed to receive new access token: {Errors}",
                        string.Join("; ", authenticationResult.Errors.Select(error => error.Message))
                    );
                    return null;
                }
            );
        }
        finally
        {
            _accessTokenRequestSemaphore.Release();
        }
    }

    public async Task<Result<MedRunnerTokenAuthentication>> AuthenticateApiTokenAsync(string apiToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiToken))
        {
            return Result.Fail<MedRunnerTokenAuthentication>("An API token is required.");
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await ApiClient.Auth.RequestTokenAsync(apiToken);
            if (!response.Success || response.Data is null)
            {
                return Result.Fail<MedRunnerTokenAuthentication>(
                    string.IsNullOrWhiteSpace(response.ErrorMessage)
                        ? $"Medrunner rejected the API token ({response.StatusCode})."
                        : response.ErrorMessage
                );
            }

            var identityResult = await ValidateAccessTokenAsync(response.Data.AccessToken);
            if (identityResult.IsFailed)
            {
                logger.LogWarning(
                    "Medrunner returned an invalid access token after API token exchange: {Errors}",
                    string.Join("; ", identityResult.Errors.Select(error => error.Message))
                );
                return Result.Fail<MedRunnerTokenAuthentication>(identityResult.Errors);
            }

            return Result.Ok(
                new MedRunnerTokenAuthentication
                {
                    AccessToken = response.Data.AccessToken,
                    RefreshToken = response.Data.RefreshToken,
                    ExpiresAt = response.Data.AccessTokenExpiration,
                    Identity = identityResult.Value,
                }
            );
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to exchange a Medrunner API token");
            return Result.Fail<MedRunnerTokenAuthentication>(exception.Message);
        }
    }

    public void ApplyAuthentication(MedRunnerTokenAuthentication authentication)
    {
        config.AccessToken = authentication.AccessToken;
        config.RefreshToken = authentication.RefreshToken;
        Identity = authentication.Identity;
        logger.LogDebug("Access token currently valid for {IdentityName}", Identity.Name);
    }

    public void ClearAuthentication()
    {
        config.AccessToken = null;
        config.RefreshToken = null;
        Identity = null;
    }

    private async Task<MedRunnerTokenAuthentication?> GetActiveAuthenticationAsync()
    {
        var accessToken = config.AccessToken;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        var identityResult = await ValidateAccessTokenAsync(accessToken);
        if (identityResult.IsFailed)
        {
            logger.LogWarning("Access token validation failed: {Errors}", string.Join("; ", identityResult.Errors.Select(error => error.Message)));
            config.AccessToken = null;
            Identity = null;
            return null;
        }

        Identity = identityResult.Value;
        return new MedRunnerTokenAuthentication
        {
            AccessToken = accessToken,
            RefreshToken = config.RefreshToken ?? string.Empty,
            ExpiresAt = DateTimeOffset.MaxValue,
            Identity = identityResult.Value,
        };
    }

    private async Task<Result<ClaimsIdentity>> ValidateAccessTokenAsync(string accessToken)
    {
        var validationResult = await _tokenHandler.ValidateTokenAsync(accessToken, _tokenValidationParameters);
        if (!validationResult.IsValid)
        {
            return Result.Fail<ClaimsIdentity>(validationResult.Exception?.Message ?? "The access token is invalid.");
        }

        return Result.Ok(validationResult.ClaimsIdentity);
    }
}
