namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using Abstractions.Endpoints;
using API.Endpoints.Auth.Request;
using Models;

public class MockAuthEndpoint : MockEndpointBase, IAuthEndpoint
{
    public Task<ApiResponse<string>> SignOutAsync(SignOutRequest? oldToken = null)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockAuthEndpoint), nameof(SignOutAsync)));

    public Task<ApiResponse<List<ApiToken>>> GetApiTokensAsync()
        => Task.FromResult(NotSupportedResponse<List<ApiToken>>(nameof(MockAuthEndpoint), nameof(GetApiTokensAsync)));

    public Task<ApiResponse<string>> CreateApiTokenAsync(CreateApiTokenRequest newToken)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockAuthEndpoint), nameof(CreateApiTokenAsync)));

    public Task<ApiResponse<string>> DeleteApiTokenAsync(string id)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockAuthEndpoint), nameof(DeleteApiTokenAsync)));

    public Task<ApiResponse<TokenGrant>> RequestTokenAsync(string refreshToken)
        => Task.FromResult(NotSupportedResponse<TokenGrant>(nameof(MockAuthEndpoint), nameof(RequestTokenAsync)));
}
