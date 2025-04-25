namespace Arkanis.Overlay.Infrastructure.Data;

using System.Text.Json;
using Converters;
using Domain.Models;
using Entities;
using Microsoft.EntityFrameworkCore;

public class OverlayDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ExternalSourceDataCache> ExternalSourceDataCache
        => Set<ExternalSourceDataCache>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ComplexProperties<ServiceAvailableState>();

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
