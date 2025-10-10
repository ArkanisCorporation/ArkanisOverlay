namespace Arkanis.Overlay.Domain.Abstractions.Services;

using System.Security.Claims;
using Common.Options;
using FluentResults;

public interface IExternalAccountContext
{
    public ClaimsIdentity Identity { get; }

    public bool IsAuthenticated { get; }

    public Result<ClaimsIdentity>? LastResult { get; }

    public Task<Result<ClaimsIdentity>> ConfigureAsync(UserPreferences.Credentials credentials, CancellationToken cancellationToken);

    public Task UpdateAsync(CancellationToken cancellationToken);

    public Task UnlinkAsync(CancellationToken cancellationToken);
}
