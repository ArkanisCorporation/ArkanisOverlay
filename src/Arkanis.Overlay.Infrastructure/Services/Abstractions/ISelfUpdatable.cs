namespace Arkanis.Overlay.Infrastructure.Services.Abstractions;

/// <summary>
///     This service can be periodically updated, and it can update itself.
/// </summary>
public interface ISelfUpdatable
{
    Task UpdateAsync(CancellationToken cancellationToken);

    Task UpdateIfNecessaryAsync(CancellationToken cancellationToken);
}
