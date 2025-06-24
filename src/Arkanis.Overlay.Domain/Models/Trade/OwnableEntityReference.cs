namespace Arkanis.Overlay.Domain.Models.Trade;

using Game;

public abstract class OwnableEntityReference(GameEntity entity)
{
    public UexApiGameEntityId EntityId { get; protected set; } = entity.Id;

    public GameEntity Entity { get; set; } = entity;
}

public sealed class ItemReference(GameItem entity) : OwnableEntityReference(entity)
{
    public new UexId<GameItem> EntityId
    {
        get => base.EntityId as UexId<GameItem> ?? throw new InvalidOperationException();
        set => base.EntityId = value;
    }

    public GameItem Item { get; set; } = entity;
}

public sealed class CommodityReference(GameCommodity entity) : OwnableEntityReference(entity)
{
    public new UexId<GameCommodity> EntityId
    {
        get => base.EntityId as UexId<GameCommodity> ?? throw new InvalidOperationException();
        set => base.EntityId = value;
    }

    public GameCommodity Commodity { get; set; } = entity;
}

public sealed class VehicleReference(GameVehicle entity) : OwnableEntityReference(entity)
{
    public new UexId<GameVehicle> EntityId
    {
        get => base.EntityId as UexId<GameVehicle> ?? throw new InvalidOperationException();
        set => base.EntityId = value;
    }

    public GameVehicle Vehicle { get; set; } = entity;
}
