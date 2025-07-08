namespace Arkanis.Overlay.Infrastructure.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abstractions;
using Converters;
using Domain.Models.Game;
using Domain.Models.Inventory;
using Domain.Models.Trade;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal abstract class InventoryEntryEntityBase(InventoryEntryBase.EntryType entryType) : IDatabaseEntity<InventoryEntryId>
{
    private readonly string? _discriminator;

    public InventoryEntryBase.EntryType EntryType { get; private init; } = entryType;

    public required QuantityOfEntity Quantity { get; set; }

    //? maximum length determined automatically by convention
    //   ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Discriminator
    {
        get => _discriminator ?? CreateDiscriminatorValueFor(EntryType);
        private init => _discriminator = value;
    }

    public TradeRunId? TradeRunId { get; set; }

    public TradeRunEntity? TradeRun { get; set; }

    public InventoryEntryListId? ListId { get; set; }

    public InventoryEntryListEntity? List { get; set; }

    [Key]
    public required InventoryEntryId Id { get; init; }

    protected static string CreateDiscriminatorValueFor(InventoryEntryBase.EntryType entryType)
        => entryType.ToString();

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

            builder.Navigation(x => x.List)
                .AutoInclude();

            builder.HasOne(x => x.Quantity)
                .WithOne()
                .HasForeignKey<QuantityOfEntity>(x => x.InventoryEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Quantity)
                .AutoInclude();

            builder.HasOne(x => x.TradeRun)
                .WithMany()
                .HasForeignKey(x => x.TradeRunId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(x => x.TradeRun)
                .AutoInclude();

            // explicit value must be defined for manual discriminator property in this case
            builder.HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(InventoryEntryBase.EntryType.Undefined));

            // allows entities to mutate types on Update
            // see: https://github.com/dotnet/efcore/issues/3786#issuecomment-161063981
            builder.Property(x => x.Discriminator)
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        }
    }
}

internal sealed class VirtualInventoryEntryEntity() : InventoryEntryEntityBase(InventoryEntryBase.EntryType.Virtual)
{
    internal new class Configuration : IEntityTypeConfiguration<VirtualInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<VirtualInventoryEntryEntity> builder)
            => builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(InventoryEntryBase.EntryType.Virtual));
    }
}

internal sealed class LocationInventoryEntryEntity() : InventoryEntryEntityBase(InventoryEntryBase.EntryType.Location), IDatabaseEntityWithLocation
{
    [Column(nameof(LocationId))]
    public required UexApiGameEntityId LocationId { get; set; }

    internal new class Configuration : IEntityTypeConfiguration<LocationInventoryEntryEntity>
    {
        public void Configure(EntityTypeBuilder<LocationInventoryEntryEntity> builder)
        {
            builder.HasBaseType<InventoryEntryEntityBase>()
                .HasDiscriminator(x => x.Discriminator)
                .HasValue(CreateDiscriminatorValueFor(InventoryEntryBase.EntryType.Location));

            builder.Property(x => x.LocationId)
                .HasConversion<UexApiDomainIdConverter>();
        }
    }
}
