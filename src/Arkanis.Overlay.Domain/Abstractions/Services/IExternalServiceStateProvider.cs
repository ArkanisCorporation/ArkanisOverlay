namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models;

public interface IExternalServiceStateProvider
{
    public Task<ExternalServiceState> LoadCurrentServiceStateAsync(CancellationToken cancellationToken);
}
