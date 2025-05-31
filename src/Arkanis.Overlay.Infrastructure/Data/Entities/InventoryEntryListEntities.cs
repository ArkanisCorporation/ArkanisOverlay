namespace Arkanis.Overlay.Infrastructure.Data.Entities;

using System.ComponentModel.DataAnnotations;
using Abstractions;
using Converters;
using Domain.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class InventoryEntryListEntity : IDatabaseEntity<InventoryEntryListId>
{
    [Required]
    [MaxLength(60)]
    public required string Name { get; set; }

    [MaxLength(10000)]
    public required string Notes { get; set; }

    public virtual List<InventoryEntryEntityBase> Entries { get; init; } = [];

    [Key]
    public required InventoryEntryListId Id { get; init; }

    internal class Configuration : IEntityTypeConfiguration<InventoryEntryListEntity>
    {
        public void Configure(EntityTypeBuilder<InventoryEntryListEntity> builder)
        {
            builder.Property(x => x.Id)
                .HasConversion<GuidDomainIdConverter<InventoryEntryListId>>();

            builder.HasMany(x => x.Entries)
                .WithMany()
                .UsingEntity<InventoryEntryListItemEntity>(
                    left => left.HasOne<InventoryEntryEntityBase>().WithMany().HasForeignKey(x => x.EntryId).HasPrincipalKey(x => x.Id),
                    left => left.HasOne<InventoryEntryListEntity>().WithMany().HasForeignKey(x => x.ListId).HasPrincipalKey(x => x.Id)
                );

            builder.Navigation(x => x.Entries)
                .AutoInclude();
        }
    }
}

[Index(nameof(ListId), nameof(EntryId), IsUnique = true)]
internal class InventoryEntryListItemEntity
{
    public required InventoryEntryListId ListId { get; init; }
    public required InventoryEntryId EntryId { get; init; }
}
