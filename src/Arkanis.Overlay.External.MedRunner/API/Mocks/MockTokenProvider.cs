namespace Arkanis.Overlay.External.MedRunner.API.Mocks;

using System.Security.Claims;
using Abstractions;

public class MockTokenProvider : IMedRunnerTokenProvider
{
    public ClaimsIdentity? Identity { get; }

    public bool IsAuthenticated { get; }

    public Task<string?> GetAccessTokenAsync()
        => GetAccessTokenAsync("unknown");

    public Task<string?> GetAccessTokenAsync(string source)
        => Task.FromResult<string?>(null);
}
