namespace Arkanis.Overlay.Domain.Abstractions.Game;

using Models.Game;

public interface IBoundToDomainEntity
{
    IDomainId EntityId { get; }
}

public interface IGameEntityPrice : IBoundToDomainEntity;

public interface IGameEntityPurchasePrice : IGameEntityPrice
{
    GameCurrency Price { get; }
}

public interface IGameEntitySalePrice : IGameEntityPrice
{
    GameCurrency Price { get; }
}

public interface IGameEntityRentalPrice : IGameEntityPrice
{
    GameCurrency Price { get; }
}
