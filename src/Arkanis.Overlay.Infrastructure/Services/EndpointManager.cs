using Timer = System.Threading.Timer;

namespace Arkanis.Overlay.Infrastructure.Services;

using Abstractions;
using API;
using Data;
using Data.Entities;
using Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class EndpointManager(
    ILogger<EndpointManager> logger,
    DataClient dataClient,
    IServiceProvider serviceProvider
)
    : IEndpointManager, IHostedService, IDisposable
{
    private readonly Dictionary<Type, EndpointConfig> _endpoints = new();

    public void Dispose()
    {
        foreach (var (_, config) in _endpoints)
        {
            config.Timer?.Dispose();
        }
    }

    public TimeSpan GetTimeUntilNextUpdate<T>() where T : BaseEntity, new()
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UEXContext>();
        var dbSet = dbContext.Set<CacheInfo>();

        var type = typeof(T);
        var config = _endpoints[type];

        var lastUpdated = dbSet
            .Where(e => e.TypeName == type.Name)
            .Select(e => e.LastUpdated)
            .FirstOrDefault();

        if (lastUpdated == default)
        {
            return TimeSpan.Zero;
        }

        var nextUpdate = lastUpdated + config.CacheTtl - DateTime.UtcNow;
        return nextUpdate <= TimeSpan.Zero ? TimeSpan.Zero : nextUpdate;
    }

    public async Task MarkAsUpdatedAsync<T>() where T : BaseEntity, new()
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UEXContext>();
        var dbSet = dbContext.Set<CacheInfo>();

        var type = typeof(T);
        var config = _endpoints[type];

        var cacheInfo = new CacheInfo
        {
            TypeName = type.Name,
            LastUpdated = DateTime.UtcNow,
            ApiPath = config.ApiPath,
        };

        dbSet.AddOrUpdate(cacheInfo);

        logger.LogInformation(
            "Marked {type} as updated, next update in {timeUntilNextUpdate}",
            type.Name,
            config.CacheTtl
        );

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task RegisterEndpointAsync<T>(string apiPath, string cacheTtl, Func<string, IEnumerable<string>>? mapper = null)
        where T : BaseEntity, new()
    {
        if (_endpoints.ContainsKey(typeof(T)))
        {
            logger.LogError(
                "Cannot register endpoint {endpoint} for type {type} because it is already registered",
                apiPath,
                typeof(T).Name
            );
            throw new InvalidOperationException("Cannot register endpoint because it is already registered");
        }

        var config = new EndpointConfig(apiPath, cacheTtl, mapper);
        _endpoints.Add(typeof(T), config);

        var timeUntilNextUpdate = GetTimeUntilNextUpdate<T>();
        if (timeUntilNextUpdate <= TimeSpan.Zero)
        {
            await UpdateEndpoint<T>().ConfigureAwait(false);
            timeUntilNextUpdate = GetTimeUntilNextUpdate<T>();
        }

        config.Timer = new Timer(
            _ => Task.Run(UpdateEndpoint<T>),
            null,
            timeUntilNextUpdate,
            config.CacheTtl
        );

        logger.LogInformation(
            "Registered endpoint {endpoint} for type {type} - Next update in {timeUntilNextUpdate}",
            apiPath,
            typeof(T).Name,
            timeUntilNextUpdate
        );
    }

    public async Task RegisterDependantEndpoint<T, TDependency>(
        string apiPath,
        string cacheTtl,
        Func<string, List<TDependency>, IEnumerable<string>> mapper
    )
        where T : BaseEntity, new()
        where TDependency : BaseEntity, new()
    {
        var type = typeof(T);
        var dependencyType = typeof(TDependency);

        if (!_endpoints.TryGetValue(dependencyType, out var dependencyConfig))
        {
            logger.LogError(
                "Cannot register dependant endpoint {endpoint} for type {type} because the " + "dependency type {dependency} is not registered",
                apiPath,
                type.Name,
                dependencyType.Name
            );

            throw new InvalidOperationException("Cannot register dependant endpoint because the dependency type is not registered");
        }

        await RegisterEndpointAsync<T>(apiPath, cacheTtl, WrappedMapper).ConfigureAwait(false);
        // add the dependency type as a dependant AFTER registering the endpoint
        // because `RegisterEndpoint` prevents duplicate registrations
        dependencyConfig.Dependents.Add(type);
        logger.LogInformation("Registered endpoint dependency: {type} -> {dependency}", type.Name, dependencyType.Name);
        return;

        IEnumerable<string> WrappedMapper(string wrapperApiPath)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UEXContext>();
            var dbSet = dbContext.Set<TDependency>();
            return mapper(wrapperApiPath, dbSet.ToList());
        }
    }

    public async Task UpdateEndpointEntities<T>(IEnumerable<T> entities) where T : BaseEntity, new()
    {
        var type = typeof(T);
        var entityList = entities.ToList();

        if (entityList.Count == 0)
        {
            logger.LogError("Failed to retrieve any entities for type {type}", type.Name);
            return;
        }

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UEXContext>();
            var dbSet = dbContext.Set<T>();
            // we are synchronizing the database from the API
            // so we can be sure that the database is always up to date
            // therefore we delete all existing data and replace it with the new data
            await dbSet.ExecuteDeleteAsync().ConfigureAwait(false);
            await dbSet.AddRangeAsync(entityList).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            // previous duct-tape solution to prevent "entity already tracked" errors
            // dbContext.ChangeTracker.Clear();

            logger.LogInformation("Updated {type} with {count} entities", type.Name, entityList.Count);
            await MarkAsUpdatedAsync<T>().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update {type}", typeof(T).Name);
        }
    }

    public async Task UpdateEndpoint<T>() where T : BaseEntity, new()
    {
        var type = typeof(T);
        var config = _endpoints[type];

        await UpdateEndpoint<T>(config.ApiPaths).ConfigureAwait(false);
    }

    public async Task UpdateEndpoint<T>(IEnumerable<string> apiPaths) where T : BaseEntity, new()
    {
        var entities = await FetchEndpoint<T>(apiPaths).ConfigureAwait(false);
        await UpdateEndpointEntities(entities).ConfigureAwait(false);
    }

    public async Task<IEnumerable<T>> FetchEndpoint<T>(IEnumerable<string> apiPaths) where T : BaseEntity, new()
    {
        var tasks = apiPaths.Select(apiPath => dataClient.Get<List<T>>(apiPath).ContinueWith(e => e.Result ?? [])).ToArray();

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        return results.SelectMany(r => r);
    }

    public void UpdateDependantEndpoints<TDependency>(IEnumerable<TDependency> entities)
        where TDependency : BaseEntity, new()
    {
        var dependencyType = typeof(TDependency);
        var config = _endpoints[dependencyType];

        if (config.Dependents.Count == 0)
        {
            return;
        }

        var dependantEndpoints = config.Dependents.Select(e => _endpoints[e]).ToList();

        foreach (var endpoint in dependantEndpoints)
        {
            // run immediately
            endpoint.Timer?.Change(TimeSpan.Zero, endpoint.CacheTtl);
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await RegisterEndpointAsync<CategoryEntity>("categories", "01.00:00:00").ConfigureAwait(false);
        await RegisterDependantEndpoint<ItemEntity, CategoryEntity>(
                "items",
                "01.00:00:00",
                (apiPath, c) => c.Where(e => e.Type == "item").Select(e => $"{apiPath}?id_category={e.Id}")
            )
            .ConfigureAwait(false);

        await RegisterEndpointAsync<CommodityEntity>("commodities", "00.01:00:00").ConfigureAwait(false);
        await RegisterEndpointAsync<VehicleEntity>("vehicles", "00.12:00:00").ConfigureAwait(false);
        await RegisterEndpointAsync<ItemsPricesAllEntity>("items_prices_all", "00.12:00:00").ConfigureAwait(false);
        await RegisterEndpointAsync<CommoditiesPricesAllEntity>("commodities_prices_all", "00.00:30:00").ConfigureAwait(false);
        await RegisterEndpointAsync<CommoditiesRawPricesAllEntity>("commodities_raw_prices_all", "00.00:30:00").ConfigureAwait(false);
        await RegisterEndpointAsync<VehiclesPurchasesPricesAllEntity>("vehicles_purchases_prices_all", "00.12:00:00").ConfigureAwait(false);
        await RegisterEndpointAsync<VehiclesRentalsPricesAllEntity>("vehicles_rentals_prices_all", "00.12:00:00").ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var (_, config) in _endpoints)
        {
            config.Timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        return Task.CompletedTask;
    }
}
