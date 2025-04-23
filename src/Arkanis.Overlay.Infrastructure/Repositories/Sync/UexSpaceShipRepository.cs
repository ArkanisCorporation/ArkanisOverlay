namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexSpaceShipRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IUexGameApi gameApi,
    UexGameDataStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexSpaceShipRepository> logger
) : UexGameEntityRepositoryBase<VehicleDTO, GameSpaceShip>(stateProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameCompany>();

    protected override async Task<UexApiResponse<ICollection<VehicleDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetVehiclesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data);
    }

    protected override double? GetSourceApiId(VehicleDTO source)
        => source.Id;

    protected override bool IncludeSourceModel(VehicleDTO sourceModel)
        => sourceModel is { Is_spaceship: 1, Is_ground_vehicle: 0 };
}
