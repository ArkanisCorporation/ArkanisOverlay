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
                .WithOne(x => x.List)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Navigation(x => x.Entries)
                .AutoInclude();
        }
    }
}
