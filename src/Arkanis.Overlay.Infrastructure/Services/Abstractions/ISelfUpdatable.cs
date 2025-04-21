namespace Arkanis.Overlay.Infrastructure.Services.Abstractions;

public interface ISelfUpdatable
{
    Task UpdateAsync(CancellationToken cancellationToken);

    Task UpdateIfNecessaryAsync(CancellationToken cancellationToken);
}
