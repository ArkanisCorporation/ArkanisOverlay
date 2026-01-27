namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Enums;
using Models.Game;

public interface IGameEntity : IIdentifiable, ISearchable
{
    public new UexApiGameEntityId Id { get; }

    public GameEntityName Name { get; }

    public GameEntityCategory EntityCategory { get; }

    IDomainId IIdentifiable.Id
        => Id;
}
