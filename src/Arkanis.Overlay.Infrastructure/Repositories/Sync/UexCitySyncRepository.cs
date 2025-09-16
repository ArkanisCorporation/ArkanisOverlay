namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Common.Abstractions.Services;
using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexCitySyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    IExternalSyncCacheProvider<UexCitySyncRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexCitySyncRepository> logger
) : UexGameEntitySyncRepositoryBase<UniverseCityDTO, GameCity>(stateProvider, cacheProvider, mapper, logger)
{
    protected override double CacheTimeFactor
        => 7;

    protected override IDependable GetDependencies()
        => dependencyResolver
            .DependsOn<GamePlanet>(this)
            .AlsoDependsOn<GameMoon>();

    protected override async Task<UexApiResponse<ICollection<UniverseCityDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetCitiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data?.Where(x => x.Is_available > 0).ToList());
    }

    protected override UexApiGameEntityId? GetSourceApiId(UniverseCityDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
