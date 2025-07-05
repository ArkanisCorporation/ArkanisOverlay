namespace Arkanis.Overlay.Domain.Models.Trade;

using Game;

public abstract class OwnableEntityReference(GameEntity entity)
{
    public UexApiGameEntityId EntityId { get; private set; } = entity.Id;

    public GameEntity Entity { get; private set; } = entity;

    public sealed class Item(GameItem entity) : OwnableEntityReference(entity)
    {
        public new UexId<GameItem> EntityId
        {
            get => base.EntityId as UexId<GameItem> ?? throw new InvalidOperationException();
            set => base.EntityId = value;
        }

        public new GameItem Entity
        {
            get => base.Entity as GameItem ?? throw new InvalidOperationException();
            set => base.Entity = value;
        }
    }

    public sealed class Commodity(GameCommodity entity) : OwnableEntityReference(entity)
    {
        public new UexId<GameCommodity> EntityId
        {
            get => base.EntityId as UexId<GameCommodity> ?? throw new InvalidOperationException();
            set => base.EntityId = value;
        }

        public new GameCommodity Entity
        {
            get => base.Entity as GameCommodity ?? throw new InvalidOperationException();
            set => base.Entity = value;
        }
    }

    public sealed class Vehicle(GameVehicle entity) : OwnableEntityReference(entity)
    {
        public new UexId<GameVehicle> EntityId
        {
            get => base.EntityId as UexId<GameVehicle> ?? throw new InvalidOperationException();
            set => base.EntityId = value;
        }

        public new GameVehicle Entity
        {
            get => base.Entity as GameVehicle ?? throw new InvalidOperationException();
            set => base.Entity = value;
        }
    }
}
