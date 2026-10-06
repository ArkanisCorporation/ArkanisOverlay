namespace Arkanis.Overlay.External.MedRunner.API.Mocks;

using System.Security.Claims;
using Abstractions;
using FluentResults;

public class MockTokenProvider : IMedRunnerTokenProvider
{
    public ClaimsIdentity? Identity { get; }

    public bool IsAuthenticated { get; }

    public Task<string?> GetAccessTokenAsync()
        => GetAccessTokenAsync("unknown");

    public Task<string?> GetAccessTokenAsync(string source)
        => Task.FromResult<string?>(null);

    public Task<Result<MedRunnerTokenAuthentication>> AuthenticateApiTokenAsync(string apiToken, CancellationToken cancellationToken)
        => Task.FromResult(Result.Fail<MedRunnerTokenAuthentication>("Mock Medrunner authentication is not available."));

    public void ApplyAuthentication(MedRunnerTokenAuthentication authentication)
    {
    }

    public void ClearAuthentication()
    {
    }
}
