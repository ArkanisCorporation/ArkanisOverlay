namespace Arkanis.Overlay.Infrastructure.Data;

using System.Text.Json;
using Converters;
using Domain.Models;
using Domain.Models.Inventory;
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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ComplexProperties<ServiceAvailableState>();
        configurationBuilder.ComplexProperties<Quantity>();

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
