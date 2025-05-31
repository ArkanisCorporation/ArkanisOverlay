namespace Arkanis.Overlay.Infrastructure.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abstractions;
using Converters;
using Domain.Enums;
using Domain.Models.Game;
using Domain.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

[Index(nameof(GameEntityId))]
internal abstract class InventoryEntryEntityBase : IDatabaseEntity<InventoryEntryId>
{
    private readonly string? _discriminator;

    public abstract UexApiGameEntityId GameEntityId { get; set; }

    public abstract GameEntityCategory GameEntityCategory { get; }

    public abstract SubType Type { get; }

    public required Quantity Quantity { get; set; }

    //? maximum length determined automatically by convention
    //   ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Discriminator
    {
        get => _discriminator ?? CreateDiscriminatorValueFor(GameEntityCategory, Type);
        private init => _discriminator = value;
    }

    [Key]
    public required InventoryEntryId Id { get; init; }

    protected static string CreateDiscriminatorValueFor(GameEntityCategory entityCategory, SubType subType)
        => $"{subType}_{entityCategory}";

    internal enum SubType
    {
        Undefined,
        Virtual,
        Physical,
    }

    internal class Configuration : IEntityTypeConfiguration<InventoryEntryEntityBase>
    {
        public void Configure(EntityTypeBuilder<InventoryEntryEntityBase> builder)
        {
            builder.Property(x => x.Id)
                .HasConversion<GuidDomainIdConverter<InventoryEntryId>>();

            builder.Property(x => x.GameEntityId)
                .HasConversion<UexApiDomainIdConverter>();

            // explicit value must be defined for manual discriminator property in this case
            builder.HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(GameEntityCategory.Undefined, SubType.Undefined));

            // allows entities to mutate types on Update
            // see: https://github.com/dotnet/efcore/issues/3786#issuecomment-161063981
            builder.Property(x => x.Discriminator)
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        }
    }
}

internal abstract class ItemInventoryEntryEntityBase : InventoryEntryEntityBase
{
    [NotMapped]
    public required UexId<GameItem> ItemId { get; set; }

    public override GameEntityCategory GameEntityCategory
        => GameEntityCategory.Item;

    public override UexApiGameEntityId GameEntityId
    {
        get => ItemId;
        set => ItemId = value as UexId<GameItem> ?? throw new InvalidOperationException($"Tried assigning incompatible entity ID to {GetType()}: {value}");
    }
}

internal sealed class VirtualItemInventoryEntryEntity : ItemInventoryEntryEntityBase
{
    public override SubType Type
        => SubType.Virtual;

    internal new class Configuration : IEntityTypeConfiguration<VirtualItemInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<VirtualItemInventoryEntryEntity> builder)
            => builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(GameEntityCategory.Item, SubType.Virtual));
    }
}

internal sealed class PhysicalItemInventoryEntryEntity : ItemInventoryEntryEntityBase, IDatabaseEntityWithLocation
{
    public override SubType Type
        => SubType.Physical;

    [Column(nameof(LocationId))]
    public required UexApiGameEntityId LocationId { get; set; }

    internal new class Configuration : IEntityTypeConfiguration<PhysicalItemInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<PhysicalItemInventoryEntryEntity> builder)
        {
            builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(GameEntityCategory.Item, SubType.Physical));

            builder.Property(x => x.LocationId)
                .HasConversion<UexApiDomainIdConverter>();
        }
    }
}

internal abstract class CommodityInventoryEntryEntityBase : InventoryEntryEntityBase
{
    [NotMapped]
    public required UexId<GameCommodity> CommodityId { get; set; }

    public override GameEntityCategory GameEntityCategory
        => GameEntityCategory.Commodity;

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
    public override SubType Type
        => SubType.Virtual;

    internal new class Configuration : IEntityTypeConfiguration<VirtualCommodityInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<VirtualCommodityInventoryEntryEntity> builder)
            => builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(GameEntityCategory.Commodity, SubType.Virtual));
    }
}

internal sealed class PhysicalCommodityInventoryEntryEntity : CommodityInventoryEntryEntityBase, IDatabaseEntityWithLocation
{
    public override SubType Type
        => SubType.Physical;

    [Column(nameof(LocationId))]
    public required UexApiGameEntityId LocationId { get; set; }

    internal new class Configuration : IEntityTypeConfiguration<PhysicalCommodityInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<PhysicalCommodityInventoryEntryEntity> builder)
        {
            builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(GameEntityCategory.Commodity, SubType.Physical));

            builder.Property(x => x.LocationId)
                .HasConversion<UexApiDomainIdConverter>();
        }
    }
}
