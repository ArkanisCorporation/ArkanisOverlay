namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models;

public interface IExternalServiceStateProvider
{
    Task<ExternalServiceState> LoadCurrentServiceStateAsync(CancellationToken cancellationToken);
}
