namespace Arkanis.Overlay.Infrastructure.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abstractions;
using Converters;
using Domain.Enums;
using Domain.Models.Game;
using Domain.Models.Inventory;
using Domain.Models.Trade;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

[Index(nameof(GameEntityId))]
internal abstract class InventoryEntryEntityBase(GameEntityCategory entityCategory, InventoryEntryBase.EntryType entryType) : IDatabaseEntity<InventoryEntryId>
{
    private readonly string? _discriminator;

    public abstract UexApiGameEntityId GameEntityId { get; set; }

    public GameEntityCategory GameEntityCategory { get; private init; } = entityCategory;

    public InventoryEntryBase.EntryType EntryType { get; private init; } = entryType;

    public QuantityOfEntity? QuantityReference { get; set; }

    public required Quantity Quantity { get; set; }

    //? maximum length determined automatically by convention
    //   ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Discriminator
    {
        get => _discriminator ?? CreateDiscriminatorValueFor(GameEntityCategory, EntryType);
        private init => _discriminator = value;
    }

    public TradeRunId? TradeRunId { get; set; }

    public TradeRunEntity? TradeRun { get; set; }

    public InventoryEntryListId? ListId { get; set; }

    public InventoryEntryListEntity? List { get; set; }

    [Key]
    public required InventoryEntryId Id { get; init; }

    protected static string CreateDiscriminatorValueFor(GameEntityCategory entityCategory, InventoryEntryBase.EntryType entryType)
        => $"{entryType}_{entityCategory}";

    internal class Configuration : IEntityTypeConfiguration<InventoryEntryEntityBase>
    {
        public void Configure(EntityTypeBuilder<InventoryEntryEntityBase> builder)
        {
            builder.Property(x => x.Id)
                .HasConversion<GuidDomainIdConverter<InventoryEntryId>>();

            builder.Property(x => x.ListId)
                .HasConversion<GuidDomainIdConverter<InventoryEntryListId>>();

            builder.Property(x => x.TradeRunId)
                .HasConversion<GuidDomainIdConverter<TradeRunId>>();

            builder.Property(x => x.GameEntityId)
                .HasConversion<UexApiDomainIdConverter>();

            builder.Navigation(x => x.List)
                .AutoInclude();

            builder.HasOne(x => x.QuantityReference)
                .WithOne()
                .HasForeignKey<QuantityOfEntity>(x => x.InventoryEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.TradeRun)
                .WithMany()
                .HasForeignKey(x => x.TradeRunId)
                .OnDelete(DeleteBehavior.Restrict);

            // explicit value must be defined for manual discriminator property in this case
            builder.HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(GameEntityCategory.Undefined, InventoryEntryBase.EntryType.Undefined));

            // allows entities to mutate types on Update
            // see: https://github.com/dotnet/efcore/issues/3786#issuecomment-161063981
            builder.Property(x => x.Discriminator)
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        }
    }
}

internal abstract class ItemInventoryEntryEntityBase(InventoryEntryBase.EntryType entryType) : InventoryEntryEntityBase(GameEntityCategory.Item, entryType)
{
    [NotMapped]
    public required UexId<GameItem> ItemId { get; set; }

    public override UexApiGameEntityId GameEntityId
    {
        get => ItemId;
        set => ItemId = value as UexId<GameItem> ?? throw new InvalidOperationException($"Tried assigning incompatible entity ID to {GetType()}: {value}");
    }
}

internal sealed class VirtualItemInventoryEntryEntity() : ItemInventoryEntryEntityBase(InventoryEntryBase.EntryType.Virtual)
{
    internal new class Configuration : IEntityTypeConfiguration<VirtualItemInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<VirtualItemInventoryEntryEntity> builder)
            => builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(GameEntityCategory.Item, InventoryEntryBase.EntryType.Virtual));
    }
}

internal sealed class PhysicalItemInventoryEntryEntity() : ItemInventoryEntryEntityBase(InventoryEntryBase.EntryType.Physical), IDatabaseEntityWithLocation
{
    [Column(nameof(LocationId))]
    public required UexApiGameEntityId LocationId { get; set; }

    internal new class Configuration : IEntityTypeConfiguration<PhysicalItemInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<PhysicalItemInventoryEntryEntity> builder)
        {
            builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(GameEntityCategory.Item, InventoryEntryBase.EntryType.Physical));

            builder.Property(x => x.LocationId)
                .HasConversion<UexApiDomainIdConverter>();
        }
    }
}

internal abstract class CommodityInventoryEntryEntityBase(InventoryEntryBase.EntryType entryType)
    : InventoryEntryEntityBase(GameEntityCategory.Commodity, entryType)
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

internal sealed class VirtualCommodityInventoryEntryEntity() : CommodityInventoryEntryEntityBase(InventoryEntryBase.EntryType.Virtual)
{
    internal new class Configuration : IEntityTypeConfiguration<VirtualCommodityInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<VirtualCommodityInventoryEntryEntity> builder)
            => builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(GameEntityCategory.Commodity, InventoryEntryBase.EntryType.Virtual));
    }
}

internal sealed class PhysicalCommodityInventoryEntryEntity()
    : CommodityInventoryEntryEntityBase(InventoryEntryBase.EntryType.Physical), IDatabaseEntityWithLocation
{
    [Column(nameof(LocationId))]
    public required UexApiGameEntityId LocationId { get; set; }

    internal new class Configuration : IEntityTypeConfiguration<PhysicalCommodityInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<PhysicalCommodityInventoryEntryEntity> builder)
        {
            builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(GameEntityCategory.Commodity, InventoryEntryBase.EntryType.Physical));

            builder.Property(x => x.LocationId)
                .HasConversion<UexApiDomainIdConverter>();
        }
    }
}
