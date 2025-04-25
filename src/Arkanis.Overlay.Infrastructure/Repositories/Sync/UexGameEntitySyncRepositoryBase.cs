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
internal abstract class UexGameEntitySyncRepositoryBase<TSource, TDomain>(
    UexServiceStateProvider stateProvider,
    IExternalSyncCacheProvider cacheProvider,
    UexApiDtoMapper mapper,
    ILogger logger
) : IGameEntityExternalSyncRepository<TDomain>
    where TSource : class
    where TDomain : class, IGameEntity
{
    protected ILogger Logger { get; } = logger;

    protected virtual double CacheTimeFactor
        => 1.0;

    public DateTimeOffset CachedUntil { get; set; }

    public async ValueTask<GameEntitySyncData<TDomain>> GetAllAsync(InternalDataState internalDataState, CancellationToken cancellationToken = default)
    {
        await GetDependencies().WaitUntilReadyAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (internalDataState is not DataCached { RefreshRequired: true })
            {
                var cachedData = await cacheProvider.LoadAsync<UexApiResponse<ICollection<TSource>>>(internalDataState, cancellationToken);
                if (cachedData is LoadedSyncDataCache<UexApiResponse<ICollection<TSource>>> loadedData)
                {
                    Logger.LogDebug("Loaded {EntityCount} cached {Type} entities", loadedData.Data.Result.Count, typeof(TDomain).Name);
                    return CreateSyncData(loadedData.Data, loadedData.State.SourcedState);
                }

                if (cachedData is AlreadyUpToDateWithCache<UexApiResponse<ICollection<TSource>>>)
                {
                    Logger.LogDebug("Loaded data for {Type} are already up to date: {@CachedData}", typeof(TDomain).Name, cachedData);
                    return new SyncDataUpToDate<TDomain>();
                }

                Logger.LogWarning("Could not load cached {Type} entities: {@CachedData}", typeof(TDomain).Name, cachedData);
            }

            var serviceState = await stateProvider.LoadCurrentServiceStateAsync(cancellationToken);
            if (serviceState is not ServiceAvailableState serviceAvailableState)
            {
                throw new ExternalApiResponseProcessingException($"Unsupported external game data state: {serviceState}");
            }

            var response = await GetInternalResponseAsync(cancellationToken).ConfigureAwait(false);
            var result = CreateSyncData(response, serviceAvailableState);

            await cacheProvider.StoreAsync(response, result.DataState, cancellationToken);
            return result;
        }
        catch (ExternalApiResponseProcessingException ex)
        {
            Logger.LogError(ex, "Failed processing response from remote API");
            return MissingSyncData<TDomain>.Instance;
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

    public async ValueTask<InternalDataState> CreateAppDataStateFor(ExternalServiceState localDataState, CancellationToken cancellationToken = default)
    {
        var externalDataState = await stateProvider.LoadCurrentServiceStateAsync(cancellationToken);
        return localDataState switch
        {
            ServiceUnavailableState => DataMissing.Instance,
            ServiceAvailableState current => externalDataState switch
            {
                ServiceAvailableState external => new DataCached(current, DateTimeOffset.UtcNow, CachedUntil)
                {
                    RefreshRequired = current.Version != external.Version || DateTimeOffset.UtcNow < CachedUntil,
                },
                _ => new DataLoaded(current, current.UpdatedAt),
            },
            _ => throw new NotSupportedException($"Unable to determine app data state from current game data state: {localDataState}"),
        };
    }

    private LoadedSyncData<TDomain> CreateSyncData(UexApiResponse<ICollection<TSource>> response, ServiceAvailableState serviceState)
    {
        var responseHeaders = response.CreateResponseHeaders();
        var cacheUntil = responseHeaders.GetCacheUntil(factor: CacheTimeFactor);
        var requestTime = responseHeaders.GetRequestTime();
        var domainEntities = response.Result.Where(IncludeSourceModel)
            .ToAsyncEnumerable()
            .SelectAwait(MapToDomainAsync)
            .Where(model => model is not null)
            .Select(model => model!);

        var dataState = new DataCached(serviceState, requestTime, cacheUntil);
        return new LoadedSyncData<TDomain>(domainEntities, dataState);
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

    private async ValueTask<TDomain?> MapToDomainAsync(TSource source)
    {
        try
        {
            var domainEntity = await mapper.ToGameEntityAsync(source);
            if (domainEntity is not TDomain resultEntity)
            {
                throw new ObjectMappingException($"Expected {typeof(TSource)} to map to {typeof(TDomain)}, got {domainEntity.GetType()} instead.", null);
            }

            return resultEntity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed mapping {Source} to {Target} from {@SourceData}", typeof(TSource).Name, typeof(TDomain).Name, source);
            return null;
        }
    }
}
