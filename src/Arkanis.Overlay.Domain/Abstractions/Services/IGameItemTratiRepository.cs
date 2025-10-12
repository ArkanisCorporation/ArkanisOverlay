namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Game;
using Models.Game;

public interface IGameItemTraitRepository : IGameEntityRepository<GameItemTrait>
{
    public ValueTask<IEnumerable<GameItemTrait>> GetAllForItemAsync(IDomainId domainId, CancellationToken cancellationToken = default);
}
