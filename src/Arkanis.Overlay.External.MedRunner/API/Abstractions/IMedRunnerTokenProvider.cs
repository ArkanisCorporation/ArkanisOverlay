namespace Arkanis.Overlay.External.MedRunner.API.Abstractions;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

public interface IMedRunnerTokenProvider
{
    public ClaimsIdentity? Identity { get; }

    [MemberNotNullWhen(true, nameof(Identity))]
    public bool IsAuthenticated { get; }

    Task<string?> GetAccessTokenAsync();

    Task<string?> GetAccessTokenAsync(string source);
}
