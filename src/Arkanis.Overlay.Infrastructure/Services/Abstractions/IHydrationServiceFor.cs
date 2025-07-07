namespace Arkanis.Overlay.Infrastructure.Services.Abstractions;

using Common.Abstractions.Services;

/// <summary>
///     A generic interface for hydration services that act as they can hydrate objects of any type.
/// </summary>
public interface IHydrationServiceFor
{
    bool Matches<T>(T entity);

    /// <summary>
    ///     Hydrates the given entity.
    /// </summary>
    /// <param name="entity">Any entity instance</param>
    /// <param name="cancellationToken">A process cancellation token</param>
    /// <typeparam name="T">A type of the entity provided</typeparam>
    /// <returns>A process task fulfilled when the entity hydration has been finished</returns>
    Task HydrateAsync<T>(T entity, CancellationToken cancellationToken = default);
}

/// <summary>
///     An interface for hydration services that can hydrate objects of a specific type.
/// </summary>
/// <remarks>
///     Creating cyclic hydration dependencies will result in startup deadlock.
/// </remarks>
/// <typeparam name="T">Targeted object type</typeparam>
public interface IHydrationServiceFor<in T> : IHydrationServiceFor, IDependable
{
    bool IHydrationServiceFor.Matches<TEntity>(TEntity entity)
        => entity is T;

    async Task IHydrationServiceFor.HydrateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
    {
        if (entity is T targetEntity && Matches(entity))
        {
            await HydrateAsync(targetEntity, cancellationToken);
        }
    }

    /// <summary>
    ///     Hydrates the given entity.
    /// </summary>
    /// <remarks>
    ///     It is unnecessary to call <see cref="IDependable.WaitUntilReadyAsync" /> before calling this method.
    ///     The method must call it internally to ensure validity of the hydrated data.
    /// </remarks>
    /// <param name="entity">An entity of targeted type</param>
    /// <param name="cancellationToken">A process cancellation token</param>
    /// <returns>A process task fulfilled when the entity hydration has been finished</returns>
    Task HydrateAsync(T entity, CancellationToken cancellationToken = default);
}
