namespace Arkanis.Overlay.Infrastructure.Repositories.Local.Specialised;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;

internal class UexPricingRepositorySpecialisationDecorator<TSource, TTarget>(IGameEntityRepository<TSource> repository)
    : RepositorySpecialisationDecoratorBase<TSource>(repository)
    where TSource : class, TTarget, IGameEntity
    where TTarget : IGameEntityPrice
{
    private Dictionary<IDomainId, TTarget[]> EntitiesByDomainId { get; set; } = [];

    protected ValueTask<ICollection<TTarget>> GetAllForOwnerAsync(IDomainId domainId, CancellationToken cancellationToken = default)
    {
        var traits = EntitiesByDomainId.GetValueOrDefault(domainId, []);
        return ValueTask.FromResult<ICollection<TTarget>>(traits);
    }

    protected override async Task UpdateAllAsyncCore(CancellationToken cancellationToken)
        => EntitiesByDomainId = await DecoratedRepository.GetAllAsync(cancellationToken)
            .OfType<TTarget>()
            .GroupBy(entity => entity.EntityId)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.ToArray(),
                cancellationToken: cancellationToken
            );
}

internal class UexPricingRepositorySpecialisationDecorator<TEntity>(IGameEntityRepository<TEntity> repository)
    : UexPricingRepositorySpecialisationDecorator<TEntity, TEntity>(repository)
    where TEntity : GameEntityPrice;
