namespace Arkanis.Overlay.Infrastructure.Services.Abstractions;

using Data.Entities;

public interface IEndpointManager
{
    TimeSpan GetTimeUntilNextUpdate<T>() where T : BaseEntity, new();

    Task MarkAsUpdatedAsync<T>() where T : BaseEntity, new();

    Task RegisterEndpointAsync<T>(string apiPath, string cacheTtl, Func<string, IEnumerable<string>>? mapper = null)
        where T : BaseEntity, new();

    Task RegisterDependantEndpoint<T, TDependency>(
        string apiPath,
        string cacheTtl,
        Func<string, List<TDependency>, IEnumerable<string>> mapper
    )
        where T : BaseEntity, new()
        where TDependency : BaseEntity, new();

    Task UpdateEndpoint<T>() where T : BaseEntity, new();
    Task UpdateEndpoint<T>(IEnumerable<string> apiPaths) where T : BaseEntity, new();
    Task<IEnumerable<T>> FetchEndpoint<T>(IEnumerable<string> apiPaths) where T : BaseEntity, new();
    Task UpdateEndpointEntities<T>(IEnumerable<T> entities) where T : BaseEntity, new();

    void UpdateDependantEndpoints<TDependency>(IEnumerable<TDependency> entities)
        where TDependency : BaseEntity, new();
}
