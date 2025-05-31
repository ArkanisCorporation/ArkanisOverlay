namespace Arkanis.Overlay.Infrastructure.Data;

using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ClientOverlayDbContextFactory(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<ClientOverlayDbContextFactory> logger
) : IDbContextFactory<OverlayDbContext>
{
    internal const string ConnectionName = "OverlayDatabase";

    public OverlayDbContext CreateDbContext()
    {
        var connectionString = configuration.GetConnectionString(ConnectionName);

        var databaseFilePath = Path.Combine(ApplicationConstants.ApplicationDataDirectory.FullName, "Overlay.db");
        connectionString ??= $"Data Source={databaseFilePath};Cache=Shared";

        logger.LogDebug("Connecting to {ConnectionString}", connectionString);
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();

        var optionsBuilder = new DbContextOptionsBuilder<OverlayDbContext>();
        optionsBuilder.UseApplicationServiceProvider(serviceProvider);
        optionsBuilder.UseLoggerFactory(loggerFactory);
        optionsBuilder.UseSqlite(connectionString);

        if (!hostEnvironment.IsProduction())
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        return new OverlayDbContext(optionsBuilder.Options);
    }
}
