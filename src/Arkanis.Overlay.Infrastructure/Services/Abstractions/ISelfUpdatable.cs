namespace Arkanis.Overlay.Infrastructure.Services.Abstractions;

using Quartz;

/// <summary>
///     This service can be periodically updated, and it can update itself.
/// </summary>
public interface ISelfUpdatable
{
    public ITrigger Trigger { get; }

    public Task UpdateAsync(CancellationToken cancellationToken);

    public Task UpdateIfNecessaryAsync(CancellationToken cancellationToken);
}
