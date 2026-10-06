namespace Arkanis.Overlay.Infrastructure.Repositories.Local.Specialised;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;

internal class UexGameItemTraitRepositorySpecialisationDecorator(IGameEntityRepository<GameItemTrait> traitRepository)
    : RepositorySpecialisationDecoratorBase<GameItemTrait>(traitRepository), IGameItemTraitRepository
{
    private Dictionary<UexId<GameItem>, GameItemTrait[]> TraitsByItemId { get; set; } = [];

    public ValueTask<IEnumerable<GameItemTrait>> GetAllForItemAsync(IDomainId domainId, CancellationToken cancellationToken = default)
    {
        if (domainId is not UexId<GameItem> itemId)
        {
            return ValueTask.FromResult(Enumerable.Empty<GameItemTrait>());
        }

        var traits = TraitsByItemId.GetValueOrDefault(itemId, []);
        return ValueTask.FromResult(traits.AsEnumerable());
    }

    protected override async Task UpdateAllAsyncCore(CancellationToken cancellationToken)
        => TraitsByItemId = await DecoratedRepository.GetAllAsync(cancellationToken)
            .GroupBy(trait => trait.ItemId)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.ToArray(),
                cancellationToken: cancellationToken
            );
}
