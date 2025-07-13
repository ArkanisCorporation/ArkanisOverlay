namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Common.Abstractions.Services;
using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexPointOfInterestSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    IExternalSyncCacheProvider<UexPointOfInterestSyncRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexPointOfInterestSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<PointOfInterestDTO, GamePointOfInterest>(stateProvider, cacheProvider, mapper, logger)
{
    protected override double CacheTimeFactor
        => 7;

    protected override IDependable GetDependencies()
        => dependencyResolver
            .DependsOn<GamePlanet>(this)
            .AlsoDependsOn<GameMoon>()
            .AlsoDependsOn<GameCity>()
            .AlsoDependsOn<GameOutpost>();

    protected override async Task<UexApiResponse<ICollection<PointOfInterestDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetPoiAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data?.Where(x => x.Is_available > 0).ToList());
    }

    protected override UexApiGameEntityId? GetSourceApiId(PointOfInterestDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
