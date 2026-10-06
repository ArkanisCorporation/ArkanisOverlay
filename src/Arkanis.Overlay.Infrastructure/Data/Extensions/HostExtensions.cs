namespace Arkanis.Overlay.Infrastructure.Data.Extensions;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public static class HostExtensions
{
    /// <summary>
    ///     Drops the database used by the provided <typeparamref name="TContext" />.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the context.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    public static async Task DropDatabaseAsync<TContext>(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();

        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
        await using var context = serviceScope.ServiceProvider.GetRequiredService<TContext>();

        logger.LogWarning("Dropping database {DatabaseName}", context.Database.GetDbConnection().Database);
        await context.Database.EnsureDeletedAsync(cancellationToken);
    }

    /// <summary>
    ///     Migrates the database used by the provided <typeparamref name="TContext" /> to the latest version.
    /// </summary>
    /// <remarks>
    ///     Calling this method requires the <typeparamref name="TContext" /> DB context to be registered in the DI container.
    ///     This operation fails otherwise.
    /// </remarks>
    /// <param name="host">The host to migrate the database for.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <typeparam name="TContext">The type of the DB context.</typeparam>
    public static async Task MigrateDatabaseAsync<TContext>(this IHost host, CancellationToken cancellationToken = default) where TContext : DbContext
        => await host.Services.MigrateDatabaseAsync<TContext>(cancellationToken: cancellationToken);

    /// <summary>
    ///     Migrates the database used by the provided <typeparamref name="TContext" /> to the latest version.
    /// </summary>
    /// <remarks>
    ///     Calling this method requires the <typeparamref name="TContext" /> DB context to be registered in the DI container.
    ///     This operation fails otherwise.
    /// </remarks>
    /// <param name="serviceProvider">
    ///     The service provider to use for resolving the <typeparamref name="TContext" /> DB
    ///     context.
    /// </param>
    /// <param name="dropDatabaseAndRetryOnFailure">Self-explanatory.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <typeparam name="TContext">The type of the DB context.</typeparam>
    public static async Task MigrateDatabaseAsync<TContext>(
        this IServiceProvider serviceProvider,
        bool dropDatabaseAndRetryOnFailure = true,
        CancellationToken cancellationToken = default
    )
        where TContext : DbContext
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();

        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
        await using var context = serviceScope.ServiceProvider.GetRequiredService<TContext>();

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        pendingMigrations = pendingMigrations.ToList();
        foreach (var pendingMigration in pendingMigrations)
        {
            logger.LogDebug("Detected pending database migration: {Migration}", pendingMigration);
        }

        logger.LogWarning(
            "Applying {PendingMigrationCount} pending migrations to database {DatabaseName}",
            pendingMigrations.Count(),
            context.Database.GetDbConnection().Database
        );
        try
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (DbException ex)
        {
            if (!dropDatabaseAndRetryOnFailure)
            {
                throw;
            }

            logger.LogError(ex, "Migration failed, wiping database and retrying...");
            await context.Database.EnsureDeletedAsync(cancellationToken);
            await MigrateDatabaseAsync<TContext>(
                serviceProvider,
                dropDatabaseAndRetryOnFailure: false,
                cancellationToken: cancellationToken
            );
        }
    }
}
