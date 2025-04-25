namespace Arkanis.Overlay.Infrastructure.Services;

using System.Text.Json;
using Data;
using Data.Entities;
using Data.Extensions;
using Domain.Abstractions.Services;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class ExternalSyncDatabaseCacheProvider<TRepository>(IDbContextFactory<OverlayDbContext> dbContextFactory)
    : IExternalSyncCacheProvider<TRepository> where TRepository : class
{
    private Type RepositoryType { get; } = typeof(TRepository);

    public async Task StoreAsync<TSource>(TSource source, AppDataCached dataState, CancellationToken cancellationToken = default)
    {
        var recordKey = CreateKey<TSource>();
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var cacheRecord = new ExternalSourceDataCache
        {
            Id = recordKey,
            Content = JsonSerializer.SerializeToDocument(source),
            DataState = dataState.DataState,
            CachedUntil = dataState.CachedUntil,
        };
        db.ExternalSourceDataCache.AddOrUpdate(cacheRecord);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<CachedSyncData<TSource>> LoadAsync<TSource>(CancellationToken cancellationToken = default)
    {
        var recordKey = CreateKey<TSource>();
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var cacheRecord = await db.ExternalSourceDataCache.SingleOrDefaultAsync(record => record.Id == recordKey, cancellationToken);
        if (cacheRecord is null)
        {
            return new MissingData<TSource>();
        }

        if (cacheRecord.CachedUntil < DateTimeOffset.UtcNow)
        {
            return new ExpiredCache<TSource>(cacheRecord.CachedUntil);
        }

        if (cacheRecord.Content.Deserialize<TSource>() is not { } cachedData)
        {
            return new UnprocessableData<TSource>();
        }

        var dataState = cacheRecord.DataState;
        var state = new AppDataCached(dataState, dataState.UpdatedAt, cacheRecord.CachedUntil);
        return new LoadedSyncData<TSource>(cachedData, state);
    }

    private string CreateKey<TSource>()
        => $"{RepositoryType.ShortDisplayName()}-{typeof(TSource).ShortDisplayName()}";
}
