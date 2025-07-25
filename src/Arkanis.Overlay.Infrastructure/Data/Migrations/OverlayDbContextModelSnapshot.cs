﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Arkanis.Overlay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    [DbContext(typeof(OverlayDbContext))]
    partial class OverlayDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.16");

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.ExternalSourceDataCache", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CachedUntil")
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("JSONB");

                    b.Property<long>("ContentSizeBytes")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.ComplexProperty<Dictionary<string, object>>("DataAvailableState", "Arkanis.Overlay.Infrastructure.Data.Entities.ExternalSourceDataCache.DataAvailableState#ServiceAvailableState", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<DateTimeOffset>("UpdatedAt")
                                .HasColumnType("TEXT");

                            b1.Property<string>("Version")
                                .IsRequired()
                                .HasMaxLength(10)
                                .HasColumnType("TEXT");
                        });

                    b.HasKey("Id");

                    b.ToTable("ExternalSourceDataCache");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.InventoryEntryEntityBase", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("TEXT");

                    b.Property<int>("EntryType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameEntityCategory")
                        .HasColumnType("INTEGER");

                    b.Property<string>("GameEntityId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ListId")
                        .HasColumnType("TEXT");

                    b.ComplexProperty<Dictionary<string, object>>("Quantity", "Arkanis.Overlay.Infrastructure.Data.Entities.InventoryEntryEntityBase.Quantity#Quantity", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<int>("Amount")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("Unit")
                                .HasColumnType("INTEGER");
                        });

                    b.HasKey("Id");

                    b.HasIndex("GameEntityId");

                    b.HasIndex("ListId");

                    b.ToTable("InventoryEntries");

                    b.HasDiscriminator().HasValue("Undefined_Undefined");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.InventoryEntryListEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasMaxLength(10000)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("InventoryLists");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.OwnableEntityReferenceEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("EntityId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("QuantityOfId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("QuantityOfId")
                        .IsUnique();

                    b.ToTable("OwnableEntityReferences", (string)null);
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.QuantityOfEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("Amount")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TradeRunStageId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Unit")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TradeRunStageId")
                        .IsUnique();

                    b.ToTable("Quantities", (string)null);
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("FinalizedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("VehicleId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Version")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TradeRuns");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+Stage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CargoTransferFee")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CargoTransferType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("FinalizedAt")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsRetry")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PricePerUnit")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("ReachedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("StartedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TradeRunId")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("TransferredAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TradeRunStages", (string)null);

                    b.HasDiscriminator().HasValue("Stage");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.PhysicalCommodityInventoryEntryEntity", b =>
                {
                    b.HasBaseType("Arkanis.Overlay.Infrastructure.Data.Entities.InventoryEntryEntityBase");

                    b.Property<string>("LocationId")
                        .IsRequired()
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("TEXT")
                        .HasColumnName("LocationId");

                    b.HasDiscriminator().HasValue("Physical_Commodity");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.PhysicalItemInventoryEntryEntity", b =>
                {
                    b.HasBaseType("Arkanis.Overlay.Infrastructure.Data.Entities.InventoryEntryEntityBase");

                    b.Property<string>("LocationId")
                        .IsRequired()
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("TEXT")
                        .HasColumnName("LocationId");

                    b.HasDiscriminator().HasValue("Physical_Item");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.VirtualCommodityInventoryEntryEntity", b =>
                {
                    b.HasBaseType("Arkanis.Overlay.Infrastructure.Data.Entities.InventoryEntryEntityBase");

                    b.HasDiscriminator().HasValue("Virtual_Commodity");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.VirtualItemInventoryEntryEntity", b =>
                {
                    b.HasBaseType("Arkanis.Overlay.Infrastructure.Data.Entities.InventoryEntryEntityBase");

                    b.HasDiscriminator().HasValue("Virtual_Item");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+AcquisitionStage", b =>
                {
                    b.HasBaseType("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+Stage");

                    b.Property<DateTimeOffset?>("AcquiredAt")
                        .HasColumnType("TEXT");

                    b.HasIndex("TradeRunId");

                    b.HasDiscriminator().HasValue("AcquisitionStage");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+SaleStage", b =>
                {
                    b.HasBaseType("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+Stage");

                    b.Property<DateTimeOffset?>("SoldAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("VehicleStoredAt")
                        .HasColumnType("TEXT");

                    b.HasIndex("TradeRunId");

                    b.HasDiscriminator().HasValue("SaleStage");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+TerminalPurchaseStage", b =>
                {
                    b.HasBaseType("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+AcquisitionStage");

                    b.Property<string>("TerminalId")
                        .IsRequired()
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("TEXT")
                        .HasColumnName("TerminalId");

                    b.ComplexProperty<Dictionary<string, object>>("UserSourcedData", "Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+TerminalPurchaseStage.UserSourcedData#TerminalData", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<int>("MaxContainerSize")
                                .ValueGeneratedOnUpdateSometimes()
                                .HasColumnType("INTEGER");

                            b1.Property<int>("StockStatus")
                                .ValueGeneratedOnUpdateSometimes()
                                .HasColumnType("INTEGER");

                            b1.Property<bool>("UserConfirmed")
                                .ValueGeneratedOnUpdateSometimes()
                                .HasColumnType("INTEGER");

                            b1.ComplexProperty<Dictionary<string, object>>("Stock", "Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+TerminalPurchaseStage.UserSourcedData#TerminalData.Stock#Quantity", b2 =>
                                {
                                    b2.IsRequired();

                                    b2.Property<int>("Amount")
                                        .ValueGeneratedOnUpdateSometimes()
                                        .HasColumnType("INTEGER");

                                    b2.Property<int>("Unit")
                                        .ValueGeneratedOnUpdateSometimes()
                                        .HasColumnType("INTEGER");
                                });
                        });

                    b.HasDiscriminator().HasValue("TerminalPurchaseStage");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+TerminalSaleStage", b =>
                {
                    b.HasBaseType("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+SaleStage");

                    b.Property<string>("TerminalId")
                        .IsRequired()
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("TEXT")
                        .HasColumnName("TerminalId");

                    b.ComplexProperty<Dictionary<string, object>>("UserSourcedData", "Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+TerminalSaleStage.UserSourcedData#TerminalData", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<int>("MaxContainerSize")
                                .ValueGeneratedOnUpdateSometimes()
                                .HasColumnType("INTEGER");

                            b1.Property<int>("StockStatus")
                                .ValueGeneratedOnUpdateSometimes()
                                .HasColumnType("INTEGER");

                            b1.Property<bool>("UserConfirmed")
                                .ValueGeneratedOnUpdateSometimes()
                                .HasColumnType("INTEGER");

                            b1.ComplexProperty<Dictionary<string, object>>("Stock", "Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+TerminalSaleStage.UserSourcedData#TerminalData.Stock#Quantity", b2 =>
                                {
                                    b2.IsRequired();

                                    b2.Property<int>("Amount")
                                        .ValueGeneratedOnUpdateSometimes()
                                        .HasColumnType("INTEGER");

                                    b2.Property<int>("Unit")
                                        .ValueGeneratedOnUpdateSometimes()
                                        .HasColumnType("INTEGER");
                                });
                        });

                    b.HasDiscriminator().HasValue("TerminalSaleStage");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.InventoryEntryEntityBase", b =>
                {
                    b.HasOne("Arkanis.Overlay.Infrastructure.Data.Entities.InventoryEntryListEntity", "List")
                        .WithMany("Entries")
                        .HasForeignKey("ListId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("List");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.OwnableEntityReferenceEntity", b =>
                {
                    b.HasOne("Arkanis.Overlay.Infrastructure.Data.Entities.QuantityOfEntity", null)
                        .WithOne("Reference")
                        .HasForeignKey("Arkanis.Overlay.Infrastructure.Data.Entities.OwnableEntityReferenceEntity", "QuantityOfId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.QuantityOfEntity", b =>
                {
                    b.HasOne("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+Stage", null)
                        .WithOne("Quantity")
                        .HasForeignKey("Arkanis.Overlay.Infrastructure.Data.Entities.QuantityOfEntity", "TradeRunStageId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+AcquisitionStage", b =>
                {
                    b.HasOne("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity", null)
                        .WithMany("Acquisitions")
                        .HasForeignKey("TradeRunId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+SaleStage", b =>
                {
                    b.HasOne("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity", null)
                        .WithMany("Sales")
                        .HasForeignKey("TradeRunId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.InventoryEntryListEntity", b =>
                {
                    b.Navigation("Entries");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.QuantityOfEntity", b =>
                {
                    b.Navigation("Reference")
                        .IsRequired();
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity", b =>
                {
                    b.Navigation("Acquisitions");

                    b.Navigation("Sales");
                });

            modelBuilder.Entity("Arkanis.Overlay.Infrastructure.Data.Entities.TradeRunEntity+Stage", b =>
                {
                    b.Navigation("Quantity")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
