namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexSpaceStationRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IUexGameApi gameApi,
    UexGameDataStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexSpaceStationRepository> logger
) : UexGameEntityRepositoryBase<UniverseSpaceStationDTO, GameSpaceStation>(stateProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver
            .DependsOn<GameCity>()
            .AlsoDependsOn<GamePlanet>()
            .AlsoDependsOn<GameMoon>();

    protected override async Task<UexApiResponse<ICollection<UniverseSpaceStationDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetSpaceStationsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data?.Where(x => x.Is_available > 0).ToList());
    }

    protected override UexApiGameEntityId? GetSourceApiId(UniverseSpaceStationDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameSpaceStation>(source.Id.Value)
            : null;
}
