namespace Arkanis.Overlay.Infrastructure.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Converters;
using Domain.Models.Game;
using Domain.Models.Trade;
using Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Riok.Mapperly.Abstractions;

[Index(nameof(EntityId))]
[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
public class OwnableEntityReferenceEntity
{
    [Key]
    [MapperIgnore]
    public Guid Id { get; init; }

    public Guid? QuantityOfId { get; init; }

    public required UexApiGameEntityId EntityId { get; init; }

    public static OwnableEntityReferenceEntity Create(OwnableEntityReference reference)
        => new()
        {
            EntityId = reference.EntityId,
        };

    internal OwnableEntityReference ToReference(UexApiDtoMapper mapper)
        => EntityId switch
        {
            UexId<GameItem> id => new OwnableEntityReference.Item(mapper.ResolveCachedGameEntity(id)),
            UexId<GameVehicle> id => new OwnableEntityReference.Vehicle(mapper.ResolveCachedGameEntity(id)),
            UexId<GameCommodity> id => new OwnableEntityReference.Commodity(mapper.ResolveCachedGameEntity(id)),
            _ => throw new InvalidOperationException(),
        };

    internal class Configuration : IEntityTypeConfiguration<OwnableEntityReferenceEntity>
    {
        public void Configure(EntityTypeBuilder<OwnableEntityReferenceEntity> builder)
        {
            builder.ToTable("OwnableEntityReferences");

            builder.Property(x => x.EntityId)
                .HasConversion<UexApiDomainIdConverter>();
        }
    }
}
