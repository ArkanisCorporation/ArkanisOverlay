namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Common.Abstractions.Services;
using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexSpaceShipSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexSpaceShipSyncRepository> cacheProvider,
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexSpaceShipSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<VehicleDTO, GameSpaceShip>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameCompany>(this);

    protected override async Task<UexApiResponse<ICollection<VehicleDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetVehiclesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(VehicleDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;

    /// <remarks>
    ///     Only spaceships must be processed by this repository.
    ///     Exception is raised otherwise on type disparity after domain object mapping.
    /// </remarks>
    protected override bool IncludeSourceModel(VehicleDTO sourceModel)
        => sourceModel is { Is_spaceship: 1, Is_ground_vehicle: 0 };
}
