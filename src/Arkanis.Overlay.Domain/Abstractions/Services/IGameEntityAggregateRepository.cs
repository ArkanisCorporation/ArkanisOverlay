namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;

/// <summary>
///     Allows working with all game entities regardless of their type.
/// </summary>
public interface IGameEntityAggregateRepository : IDependable
{
    public IAsyncEnumerable<IGameEntity> GetAllAsync(CancellationToken cancellationToken = default);
}
