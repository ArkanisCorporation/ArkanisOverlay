namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using Abstractions.Endpoints;
using API.Endpoints.Auth.Request;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Models;

public class MockAuthEndpoint : MockApiEndpoint, IAuthEndpoint
{
    private const string Issuer = "medrunner.space";

    private readonly JsonWebTokenHandler _tokenHandler = new();

    public Task<ApiResponse<string>> SignOutAsync(SignOutRequest? oldToken = null)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockAuthEndpoint), nameof(SignOutAsync)));

    public Task<ApiResponse<List<ApiToken>>> GetApiTokensAsync()
        => Task.FromResult(NotSupportedResponse<List<ApiToken>>(nameof(MockAuthEndpoint), nameof(GetApiTokensAsync)));

    public Task<ApiResponse<string>> CreateApiTokenAsync(CreateApiTokenRequest newToken)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockAuthEndpoint), nameof(CreateApiTokenAsync)));

    public Task<ApiResponse<string>> DeleteApiTokenAsync(string id)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockAuthEndpoint), nameof(DeleteApiTokenAsync)));

    public Task<ApiResponse<TokenGrant>> RequestTokenAsync(string refreshToken)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Expires = DateTime.UtcNow.AddMinutes(10),
            NotBefore = DateTime.UtcNow,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.UniqueName] = "TheKronnY",
                ["role"] = "client",
            },
        };
        return OkResponseAsync(
            new TokenGrant
            {
                AccessToken = _tokenHandler.CreateToken(tokenDescriptor),
                RefreshToken = refreshToken,
                AccessTokenExpiration = tokenDescriptor.Expires.Value,
            }
        );
    }
}
