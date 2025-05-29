namespace Arkanis.Overlay.Infrastructure.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Converters;
using Domain.Models.Game;
using Domain.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

[Index(nameof(GameEntityId))]
internal abstract class InventoryEntryEntityBase
{
    [Key]
    public required InventoryEntryId Id { get; init; }

    public abstract UexApiGameEntityId GameEntityId { get; set; }

    public required Quantity Quantity { get; set; }

    internal class Configuration : IEntityTypeConfiguration<InventoryEntryEntityBase>
    {
        public void Configure(EntityTypeBuilder<InventoryEntryEntityBase> builder)
        {
            builder.Property(x => x.Id)
                .HasConversion<GuidDomainIdConverter<InventoryEntryId>>();

            builder.Property(x => x.GameEntityId)
                .HasConversion<UexApiDomainIdConverter>();
        }
    }
}

internal class ItemInventoryEntryEntityBase : InventoryEntryEntityBase
{
    [NotMapped]
    public required UexId<GameItem> ItemId { get; set; }

    public override UexApiGameEntityId GameEntityId
    {
        get => ItemId;
        set => ItemId = value as UexId<GameItem> ?? throw new InvalidOperationException($"Tried assigning incompatible entity ID to {GetType()}: {value}");
    }
}

internal sealed class VirtualItemInventoryEntryEntity : ItemInventoryEntryEntityBase
{
    internal new class Configuration : IEntityTypeConfiguration<VirtualItemInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<VirtualItemInventoryEntryEntity> builder)
            => builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator();
    }
}

internal sealed class PhysicalItemInventoryEntryEntity : ItemInventoryEntryEntityBase
{
    public required UexApiGameEntityId LocationId { get; set; }

    internal new class Configuration : IEntityTypeConfiguration<PhysicalItemInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<PhysicalItemInventoryEntryEntity> builder)
        {
            builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator();

            builder.Property(x => x.LocationId)
                .HasConversion<UexApiDomainIdConverter>();
        }
    }
}

internal class CommodityInventoryEntryEntityBase : InventoryEntryEntityBase
{
    [NotMapped]
    public required UexId<GameCommodity> CommodityId { get; set; }

    public override UexApiGameEntityId GameEntityId
    {
        get => CommodityId;
        set
            => CommodityId = value as UexId<GameCommodity>
                             ?? throw new InvalidOperationException($"Tried assigning incompatible entity ID to {GetType()}: {value}");
    }
}

internal sealed class VirtualCommodityInventoryEntryEntity : CommodityInventoryEntryEntityBase
{
    internal new class Configuration : IEntityTypeConfiguration<VirtualCommodityInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<VirtualCommodityInventoryEntryEntity> builder)
            => builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator();
    }
}

internal sealed class PhysicalCommodityInventoryEntryEntity : CommodityInventoryEntryEntityBase
{
    public required UexApiGameEntityId LocationId { get; set; }

    internal new class Configuration : IEntityTypeConfiguration<PhysicalCommodityInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<PhysicalCommodityInventoryEntryEntity> builder)
        {
            builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator();

            builder.Property(x => x.LocationId)
                .HasConversion<UexApiDomainIdConverter>();
        }
    }
}
