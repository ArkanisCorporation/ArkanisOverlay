namespace Arkanis.Overlay.Infrastructure.Services.Abstractions;

using Quartz;

/// <summary>
///     This service can be periodically updated, and it can update itself.
/// </summary>
public interface ISelfUpdatable
{
    ITrigger Trigger { get; }

    Task UpdateAsync(CancellationToken cancellationToken);

    Task UpdateIfNecessaryAsync(CancellationToken cancellationToken);
}
