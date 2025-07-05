namespace Arkanis.Overlay.Infrastructure.Data.Entities;

using System.ComponentModel.DataAnnotations;
using Domain.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Riok.Mapperly.Abstractions;

public class QuantityOfEntity
{
    [Key]
    [MapperIgnore]
    public Guid Id { get; init; }

    [MapperIgnore]
    public int? TradeRunStageId { get; init; }

    public int Amount { get; set; }
    public Quantity.UnitType Unit { get; set; }
    public required OwnableEntityReferenceEntity Reference { get; init; }

    internal class Configuration : IEntityTypeConfiguration<QuantityOfEntity>
    {
        public void Configure(EntityTypeBuilder<QuantityOfEntity> builder)
        {
            builder.ToTable("Quantities");

            builder.HasOne(x => x.Reference)
                .WithOne()
                .HasForeignKey<OwnableEntityReferenceEntity>(x => x.QuantityOfId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Reference)
                .AutoInclude();
        }
    }
}
