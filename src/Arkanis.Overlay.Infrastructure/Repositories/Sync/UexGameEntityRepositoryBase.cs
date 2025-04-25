namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using System.Diagnostics.CodeAnalysis;
using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;
using External.UEX.Abstractions;
using External.UEX.Extensions;
using Infrastructure.Exceptions;
using Local;
using Microsoft.Extensions.Logging;

/// <summary>
///     A generic synchronization repository for game entities sourced from UEX API.
///     This repository handles shared logic for all UEX API endpoints.
/// </summary>
/// <param name="stateProvider">A global UEX game version provider</param>
/// <param name="mapper">A mapper for the external DTO types</param>
/// <param name="logger">A logger</param>
/// <typeparam name="TSource">External DTO type</typeparam>
/// <typeparam name="TDomain">Internal domain type</typeparam>
internal abstract class UexGameEntityRepositoryBase<TSource, TDomain>(
    UexGameDataStateProvider stateProvider,
    IExternalSyncCacheProvider cacheProvider,
    UexApiDtoMapper mapper,
    ILogger logger
) : IGameEntityExternalSyncRepository<TDomain>
    where TSource : class
    where TDomain : class, IGameEntity
{
    protected ILogger Logger { get; } = logger;

    public DateTimeOffset CachedUntil { get; set; }

    public async ValueTask<AppDataState> CreateAppDataStateFor(GameDataState localDataState, CancellationToken cancellationToken = default)
    {
        var externalDataState = await stateProvider.LoadCurrentDataState(cancellationToken);
        return localDataState switch
        {
            MissingGameDataState => AppDataMissing.Instance,
            SyncedGameDataState current => externalDataState switch
            {
                SyncedGameDataState external => new AppDataCached(current, DateTimeOffset.UtcNow, CachedUntil)
                {
                    RefreshRequired = current.Version != external.Version || DateTimeOffset.UtcNow < CachedUntil,
                },
                _ => new AppDataLoaded(current, current.UpdatedAt),
            },
            _ => throw new NotSupportedException($"Unable to determine app data state from current game data state: {localDataState}"),
        };
    }

    public async ValueTask<GameEntitySyncData<TDomain>> GetAllAsync(AppDataState appDataState, CancellationToken cancellationToken = default)
    {
        await GetDependencies().WaitUntilReadyAsync(cancellationToken).ConfigureAwait(false);
        if (appDataState is not AppDataCached { RefreshRequired: true })
        {
            var cachedData = await cacheProvider.LoadAsync<UexApiResponse<ICollection<TSource>>>(cancellationToken);
            if (cachedData is LoadedSyncData<UexApiResponse<ICollection<TSource>>> loadedData)
            {
                Logger.LogDebug("Loaded {EntityCount} cached {Type} entities", loadedData.Data.Result.Count, typeof(TDomain).Name);
                return CreateSyncData(loadedData.Data, loadedData.State.DataState);
            }

            Logger.LogDebug("Could not load cached {Type} entities: {@CachedData}", typeof(TDomain).Name, cachedData);
        }

        try
        {
            var dataState = await stateProvider.LoadCurrentDataState(cancellationToken);
            if (dataState is not SyncedGameDataState syncedData)
            {
                throw new ExternalApiResponseProcessingException($"Unsupported external game data state: {dataState}");
            }

            var response = await GetInternalResponseAsync(cancellationToken).ConfigureAwait(false);
            var result = CreateSyncData(response, dataState);

            var newAppDataState = new AppDataCached(syncedData, DateTimeOffset.UtcNow, result.CacheUntil);
            await cacheProvider.StoreAsync(response, newAppDataState, cancellationToken);
            return result;
        }
        catch (ExternalApiResponseProcessingException ex)
        {
            Logger.LogError(ex, "Failed processing response from remote API");
            return new GameEntitySyncData<TDomain>(
                new List<TDomain>().ToAsyncEnumerable(),
                MissingGameDataState.Instance,
                DateTimeOffset.Now + TimeSpan.FromMinutes(15)
            );
        }
    }

    public async Task<TDomain?> GetAsync(IDomainId id, CancellationToken cancellationToken = default)
    {
        await GetDependencies().WaitUntilReadyAsync(cancellationToken).ConfigureAwait(false);
        var result = await GetSingleInternalAsync(id, cancellationToken).ConfigureAwait(false);
        return result is not null
            ? await MapToDomainAsync(result)
            : null;
    }

    private GameEntitySyncData<TDomain> CreateSyncData(UexApiResponse<ICollection<TSource>> response, GameDataState dataState)
    {
        var cacheUntil = response.CreateResponseHeaders().GetCacheUntil();
        var domainEntities = response.Result.Where(IncludeSourceModel).ToAsyncEnumerable().SelectAwait(MapToDomainAsync);
        return new GameEntitySyncData<TDomain>(domainEntities, dataState, cacheUntil);
    }

    protected virtual bool IncludeSourceModel(TSource sourceModel)
        => true;

    protected virtual IDependable GetDependencies()
        => NoDependency.Instance;

    [DoesNotReturn]
    protected static ICollection<TSource> ThrowCouldNotParseResponse()
        => throw new ExternalApiResponseProcessingException($"Failed to parse response for {typeof(TSource)} from UEX API.");

    protected static UexApiResponse<ICollection<TSource>> CreateResponse(UexApiResponse? response, ICollection<TSource>? items)
        => response is not null
            ? new UexApiResponse<ICollection<TSource>>(response.StatusCode, response.Headers, items ?? ThrowCouldNotParseResponse())
            : new UexApiResponse<ICollection<TSource>>(0, new Dictionary<string, IEnumerable<string>>(), []);

    protected abstract Task<UexApiResponse<ICollection<TSource>>> GetInternalResponseAsync(CancellationToken cancellationToken);

    protected abstract UexApiGameEntityId? GetSourceApiId(TSource source);

    private async Task<TSource?> GetSingleInternalAsync(IDomainId id, CancellationToken cancellationToken)
    {
        if (id is not UexApiGameEntityId uexApiId)
        {
            throw new NotSupportedException($"UEX API request cannot be performed based on entity ID of: {id.GetType()}");
        }

        var response = await GetInternalResponseAsync(cancellationToken).ConfigureAwait(false);
        return response.Result.FirstOrDefault(source => uexApiId.Equals(GetSourceApiId(source)));
    }

    private async ValueTask<TDomain> MapToDomainAsync(TSource source)
    {
        var domainEntity = await mapper.ToGameEntityAsync(source);
        if (domainEntity is not TDomain resultEntity)
        {
            throw new ObjectMappingException($"Expected {typeof(TSource)} to map to {typeof(TDomain)}, got {domainEntity.GetType()} instead.", null);
        }

        return resultEntity;
    }
}
