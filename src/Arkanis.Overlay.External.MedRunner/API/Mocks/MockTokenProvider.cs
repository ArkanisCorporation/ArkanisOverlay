namespace Arkanis.Overlay.External.MedRunner.API.Mocks;

using System.Security.Claims;
using Abstractions;
using Models;

public class MockTokenProvider : IMedRunnerTokenProvider
{
    public ClaimsIdentity? Identity { get; }

    public bool IsAuthenticated { get; }

    public Task<string?> GetAccessTokenAsync()
        => GetAccessTokenAsync("unknown");

    public Task<string?> GetAccessTokenAsync(string source)
        => Task.FromResult<string?>(null);

    public Task<string?> GetRefreshTokenAsync()
        => Task.FromResult<string?>(null);

    public Task SetTokensAsync(TokenGrant tokens)
        => Task.CompletedTask;

    public Task ClearTokensAsync()
        => Task.CompletedTask;

    public Task<bool> HasValidTokensAsync()
        => Task.FromResult(false);
}
