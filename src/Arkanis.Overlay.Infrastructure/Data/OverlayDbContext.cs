namespace Arkanis.Overlay.Infrastructure.Data;

using System.Text.Json;
using Converters;
using Domain.Models;
using Domain.Models.Game;
using Domain.Models.Inventory;
using Domain.Models.Trade;
using Entities;
using Microsoft.EntityFrameworkCore;

public class OverlayDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ExternalSourceDataCache> ExternalSourceDataCache
        => Set<ExternalSourceDataCache>();

    internal DbSet<InventoryEntryEntityBase> InventoryEntries
        => Set<InventoryEntryEntityBase>();

    internal DbSet<InventoryEntryListEntity> InventoryLists
        => Set<InventoryEntryListEntity>();

    internal DbSet<TradeRunEntity> TradeRuns
        => Set<TradeRunEntity>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ComplexProperties<ServiceAvailableState>();
        configurationBuilder.ComplexProperties<Quantity>();
        configurationBuilder.ComplexProperties<TerminalData>();

        configurationBuilder.Properties<GameCurrency>()
            .HaveConversion<GameCurrencyValueConverter>();

        configurationBuilder.Properties<StarCitizenVersion>()
            .HaveConversion<StarCitizenVersionValueConverter>()
            .HaveMaxLength(10);

        configurationBuilder.Properties<JsonDocument>()
            .HaveConversion<JsonDocumentValueConverter>()
            .HaveColumnType("JSONB"); // https://sqlite.org/jsonb.html

        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OverlayDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
