namespace Arkanis.Overlay.Infrastructure.Data.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public static class HostExtensions
{
    /// <summary>
    ///     This method will apply any pending migrations to the database.
    /// </summary>
    /// <remarks>
    ///     Calling this method requires the <typeparamref name="TContext" /> DB context to be registered in the DI container.
    ///     This operation fails otherwise.
    /// </remarks>
    /// <param name="host"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TContext"></typeparam>
    public static async Task MigrateDatabaseAsync<TContext>(this IHost host, CancellationToken cancellationToken = default) where TContext : DbContext
    {
        await using var serviceScope = host.Services.CreateAsyncScope();

        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
        await using var context = serviceScope.ServiceProvider.GetRequiredService<TContext>();

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        pendingMigrations = pendingMigrations.ToList();
        foreach (var pendingMigration in pendingMigrations)
        {
            logger.LogDebug("Detected pending database migration: {Migration}", pendingMigration);
        }

        logger.LogInformation("Applying {PendingMigrationCount} pending migrations", pendingMigrations.Count());
        await context.Database.MigrateAsync(cancellationToken);
    }
}
