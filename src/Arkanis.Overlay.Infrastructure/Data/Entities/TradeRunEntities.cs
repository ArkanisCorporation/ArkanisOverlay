namespace Arkanis.Overlay.Infrastructure.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Abstractions;
using Converters;
using Domain.Models;
using Domain.Models.Game;
using Domain.Models.Trade;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Riok.Mapperly.Abstractions;

[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
public class TradeRunEntity : IDatabaseEntity<TradeRunId>
{
    public UexId<GameVehicle>? VehicleId { get; init; }
    public StarCitizenVersion? Version { get; init; }

    public List<AcquisitionStage> Acquisitions { get; set; } = [];
    public List<SaleStage> Sales { get; set; } = [];

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? FinalizedAt { get; set; }

    [Key]
    public TradeRunId Id { get; init; } = TradeRunId.CreateNew();

    internal class Configuration : IEntityTypeConfiguration<TradeRunEntity>
    {
        public void Configure(EntityTypeBuilder<TradeRunEntity> builder)
        {
            builder.Property(x => x.Id)
                .HasConversion<GuidDomainIdConverter<TradeRunId>>();

            builder.Property(x => x.VehicleId)
                .HasConversion<UexApiDomainIdConverter<GameVehicle>>();

            builder.Property(x => x.Version)
                .HasConversion<StarCitizenVersionValueConverter>();

            builder.HasMany(x => x.Acquisitions)
                .WithOne()
                .HasForeignKey(x => x.TradeRunId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Acquisitions)
                .AutoInclude();

            builder.HasMany(x => x.Sales)
                .WithOne()
                .HasForeignKey(x => x.TradeRunId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Sales)
                .AutoInclude();
        }
    }

    public abstract class Stage
    {
        [Key]
        public int Id { get; init; }

        [MapperIgnore]
        public TradeRunId TradeRunId { get; init; } = null!;

        public GameCurrency PricePerUnit { get; set; } = GameCurrency.Zero;

        public required QuantityOfEntity Quantity { get; set; }

        public GameCurrency CargoTransferFee { get; set; } = GameCurrency.Zero;
        public GameCargoTransferType CargoTransferType { get; set; }

        public bool IsRetry { get; set; }

        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? ReachedAt { get; set; }
        public DateTimeOffset? TransferredAt { get; set; }
        public DateTimeOffset? FinalizedAt { get; set; }

        internal class Configuration : IEntityTypeConfiguration<Stage>
        {
            public void Configure(EntityTypeBuilder<Stage> builder)
            {
                builder.ToTable("TradeRunStages");

                builder.Property(x => x.TradeRunId)
                    .HasConversion<GuidDomainIdConverter<TradeRunId>>();

                builder.HasOne(x => x.Quantity)
                    .WithOne()
                    .HasForeignKey<QuantityOfEntity>(x => x.TradeRunStageId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.Navigation(x => x.Quantity)
                    .AutoInclude();
            }
        }
    }

    public abstract class AcquisitionStage : Stage
    {
        public DateTimeOffset? AcquiredAt { get; set; }

        internal new class Configuration : IEntityTypeConfiguration<AcquisitionStage>
        {
            public void Configure(EntityTypeBuilder<AcquisitionStage> builder)
                => builder.HasBaseType<Stage>();
        }
    }

    public sealed class TerminalPurchaseStage : AcquisitionStage
    {
        [Column(nameof(TerminalId))]
        public required UexId<GameTerminal> TerminalId { get; set; }

        [Column(nameof(UserSourcedData))]
        public TerminalData UserSourcedData { get; set; } = new();

        internal new class Configuration : IEntityTypeConfiguration<TerminalPurchaseStage>
        {
            public void Configure(EntityTypeBuilder<TerminalPurchaseStage> builder)
            {
                builder.HasBaseType<AcquisitionStage>();

                builder.Property(x => x.TerminalId)
                    .HasConversion<UexApiDomainIdConverter<GameTerminal>>();
            }
        }
    }

    public abstract class SaleStage : Stage
    {
        public DateTimeOffset? SoldAt { get; set; }

        internal new class Configuration : IEntityTypeConfiguration<SaleStage>
        {
            public void Configure(EntityTypeBuilder<SaleStage> builder)
                => builder.HasBaseType<Stage>();
        }
    }

    public sealed class TerminalSaleStage : SaleStage
    {
        [Column(nameof(TerminalId))]
        public required UexId<GameTerminal> TerminalId { get; set; }

        [Column(nameof(UserSourcedData))]
        public TerminalData UserSourcedData { get; set; } = new();

        internal new class Configuration : IEntityTypeConfiguration<TerminalSaleStage>
        {
            public void Configure(EntityTypeBuilder<TerminalSaleStage> builder)
            {
                builder.HasBaseType<SaleStage>();

                builder.Property(x => x.TerminalId)
                    .HasConversion<UexApiDomainIdConverter<GameTerminal>>();
            }
        }
    }
}
