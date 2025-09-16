namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Common.Abstractions.Services;
using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexSpaceStationSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexSpaceStationSyncRepository> cacheProvider,
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexSpaceStationSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<UniverseSpaceStationDTO, GameSpaceStation>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver
            .DependsOn<GameCity>(this)
            .AlsoDependsOn<GamePlanet>()
            .AlsoDependsOn<GameMoon>();

    protected override double CacheTimeFactor
        => 7;

    protected override async Task<UexApiResponse<ICollection<UniverseSpaceStationDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetSpaceStationsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data?.Where(x => x.Is_available > 0).ToList());
    }

    protected override UexApiGameEntityId? GetSourceApiId(UniverseSpaceStationDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
