namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Common.Abstractions.Services;
using Game;

/// <summary>
///     Allows working with all game entities regardless of their type.
/// </summary>
public interface IGameEntityAggregateRepository : IDependable
{
    IAsyncEnumerable<IGameEntity> GetAllAsync(CancellationToken cancellationToken = default);
}
