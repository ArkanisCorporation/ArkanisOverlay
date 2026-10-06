namespace Arkanis.Overlay.Domain.Models.Game;

using System.ComponentModel;
using Abstractions.Game;
using Attributes;
using Enums;

public abstract class GameEntityPrice(UexApiGameEntityId id, UexApiGameEntityId ownerId)
    : GameEntity(id, GameEntityCategory.Price), IGameEntityPrice
{
    public UexApiGameEntityId OwnerId { get; } = ownerId;

    public required DateTimeOffset UpdatedAt { get; init; }

    IDomainId IBoundToDomainEntity.EntityId
        => OwnerId;
}

public abstract class GameEntityTerminalPrice(UexApiGameEntityId id, UexApiGameEntityId ownerId, GameTerminal terminal)
    : GameEntityPrice(id, ownerId), IGameLocatedAt
{
    public GameTerminal Terminal
        => terminal;

    IGameLocation IGameLocatedAt.Location
        => terminal;
}

[Description("Game Purchase Price Entry")]
[CacheEntryDescription("Game Purchase Prices")]
public sealed class GameEntityPurchasePrice(
    UexApiGameEntityId id,
    UexApiGameEntityId ownerId,
    GameCurrency purchasePrice,
    GameTerminal terminal
) : GameEntityTerminalPrice(id, ownerId, terminal), IGameEntityPurchasePrice
{
    public override GameEntityName Name { get; } = new(new GameEntityName.Name("Purchase Price"));

    public GameCurrency PurchasePrice { get; } = purchasePrice;

    GameCurrency IGameEntityPurchasePrice.Price
        => PurchasePrice;
}

[Description("Game Sale Price Entry")]
[CacheEntryDescription("Game Sale Prices")]
public sealed class GameEntitySalePrice(
    UexApiGameEntityId id,
    UexApiGameEntityId ownerId,
    GameCurrency salePrice,
    GameTerminal terminal
) : GameEntityTerminalPrice(id, ownerId, terminal), IGameEntitySalePrice
{
    public override GameEntityName Name { get; } = new(new GameEntityName.Name("Sale Price"));

    public GameCurrency SalePrice { get; } = salePrice;

    GameCurrency IGameEntitySalePrice.Price
        => SalePrice;
}

[Description("Game Trade Price Entry")]
[CacheEntryDescription("Game Trade Prices")]
public sealed class GameEntityTradePrice(
    UexApiGameEntityId id,
    UexApiGameEntityId ownerId,
    GameCurrency purchasePrice,
    GameCurrency salePrice,
    GameTerminal terminal
) : GameEntityTerminalPrice(id, ownerId, terminal), IGameEntityPurchasePrice, IGameEntitySalePrice
{
    public override GameEntityName Name { get; } = new(new GameEntityName.Name("Trade Price"));

    public GameCurrency PurchasePrice { get; } = purchasePrice;
    public GameCurrency SalePrice { get; } = salePrice;

    GameCurrency IGameEntityPurchasePrice.Price
        => PurchasePrice;

    GameCurrency IGameEntitySalePrice.Price
        => SalePrice;
}

[Description("Game Rental Price Entry")]
[CacheEntryDescription("Game Rental Prices")]
public sealed class GameEntityRentalPrice(
    UexApiGameEntityId id,
    UexApiGameEntityId ownerId,
    GameCurrency rentalPrice,
    GameTerminal terminal
) : GameEntityTerminalPrice(id, ownerId, terminal), IGameEntityRentalPrice
{
    public override GameEntityName Name { get; } = new(new GameEntityName.Name("Rental Price"));

    public GameCurrency RentalPrice { get; } = rentalPrice;

    GameCurrency IGameEntityRentalPrice.Price
        => RentalPrice;
}

[Description("External Market Price Entry")]
[CacheEntryDescription("External Market Prices")]
public class GameEntityMarketPrice(
    UexApiGameEntityId id,
    UexApiGameEntityId ownerId,
    string marketName
) : GameEntityPrice(id, ownerId)
{
    public override GameEntityName Name { get; } = new(new GameEntityName.Name("Market Price"));

    public string MarketName { get; } = marketName;
}

public sealed class GameEntityMarketPurchasePrice(
    UexApiGameEntityId id,
    UexApiGameEntityId ownerId,
    string marketName,
    GameCurrency purchasePrice
) : GameEntityMarketPrice(id, ownerId, marketName), IGameEntityPurchasePrice
{
    public override GameEntityName Name { get; } = new(new GameEntityName.Name("Market Purchase Price"));

    public GameCurrency PurchasePrice { get; } = purchasePrice;

    GameCurrency IGameEntityPurchasePrice.Price
        => PurchasePrice;
}

public sealed class GameEntityMarketSalePrice(
    UexApiGameEntityId id,
    UexApiGameEntityId ownerId,
    string marketName,
    GameCurrency salePrice
) : GameEntityMarketPrice(id, ownerId, marketName), IGameEntitySalePrice
{
    public override GameEntityName Name { get; } = new(new GameEntityName.Name("Market Sale Price"));

    public GameCurrency SalePrice { get; } = salePrice;

    GameCurrency IGameEntitySalePrice.Price
        => SalePrice;
}
