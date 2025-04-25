namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexPlanetSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexPlanetSyncRepository> cacheProvider,
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexPlanetSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<UniversePlanetDTO, GamePlanet>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameStarSystem>(this);

    protected override double CacheTimeFactor
        => 7;

    protected override async Task<UexApiResponse<ICollection<UniversePlanetDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetPlanetsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data?.Where(x => x.Is_available > 0).ToList());
    }

    protected override UexApiGameEntityId? GetSourceApiId(UniversePlanetDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GamePlanet>(source.Id.Value)
            : null;
}
