namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Models.Game;

public interface IBoundToDomainEntity
{
    public IDomainId EntityId { get; }
}

public interface IGameEntityPrice : IBoundToDomainEntity;

public interface IGameEntityPurchasePrice : IGameEntityPrice
{
    public GameCurrency Price { get; }
}

public interface IGameEntitySalePrice : IGameEntityPrice
{
    public GameCurrency Price { get; }
}

public interface IGameEntityRentalPrice : IGameEntityPrice
{
    public GameCurrency Price { get; }
}
