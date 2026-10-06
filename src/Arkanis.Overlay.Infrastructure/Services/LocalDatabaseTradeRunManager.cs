namespace Arkanis.Overlay.Infrastructure.Services;

using Abstractions;
using Data;
using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Trade;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

internal sealed class LocalDatabaseTradeRunManager(
    IDbContextFactory<OverlayDbContext> dbContextFactory,
    IMemoryCache memoryCache,
    IChangeTokenManager changeTokenManager,
    TradeRunEntityMapper mapper
) : ITradeRunManager
{
    public IChangeToken ChangeToken
        => changeTokenManager.GetChangeTokenFor<CacheId>();

    public async Task<int> GetInProgressCountAsync(CancellationToken cancellationToken = default)
        => await memoryCache.GetOrCreateAsync(
            CacheId.InProgressCountQuery,
            async entry =>
            {
                entry.AddExpirationToken(ChangeToken);
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));

                await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                return await dbContext.TradeRuns.CountAsync(x => x.FinalizedAt == null, cancellationToken);
            }
        );

    public async Task AddOrUpdateEntryAsync(TradeRun entry, CancellationToken cancellationToken = default)
    {
        var entity = mapper.Map(entry);
        await using (var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            await dbContext.TradeRuns.Where(x => x.Id == entry.Id).ExecuteDeleteAsync(cancellationToken);
            await dbContext.TradeRuns.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await TriggerChangeAsync();
    }

    public async Task DeleteRunAsync(TradeRunId entryId, CancellationToken cancellationToken = default)
    {
        await using (var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            await dbContext.TradeRuns.Where(x => x.Id == entryId).ExecuteDeleteAsync(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await TriggerChangeAsync();
    }

    public async Task<TradeRun?> GetRunAsync(TradeRunId runId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.TradeRuns.FindAsync([runId], cancellationToken);
        return entity is not null
            ? mapper.Map(entity)
            : null;
    }

    public async Task<ICollection<TradeRun>> GetAllRunsAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await dbContext.TradeRuns.ToArrayAsync(cancellationToken);
        return entities.Select(mapper.Map).ToArray();
    }

    public async Task<ICollection<TradeRun>> GetInProgressRunsAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await dbContext.TradeRuns
            .Where(x => x.FinalizedAt == null)
            .ToArrayAsync(cancellationToken);

        return entities.Select(mapper.Map).ToArray();
    }

    private async Task TriggerChangeAsync()
        => await changeTokenManager.TriggerChangeForAsync<CacheId>();

    private class CacheId
    {
        public static readonly GetCount InProgressCountQuery = new();

        public static readonly GetAll AllRunsQuery = new();

        public record GetCount;

        public record GetAll;
    }
}
