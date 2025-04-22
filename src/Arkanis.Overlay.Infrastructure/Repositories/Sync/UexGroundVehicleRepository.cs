namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexGroundVehicleRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IUexGameApi gameApi,
    UexGameDataStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexGroundVehicleRepository> logger
) : UexGameEntityRepositoryBase<VehicleDTO, GameGroundVehicle>(stateProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameCompany>();

    protected override async Task<UexApiResponse<ICollection<VehicleDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetVehiclesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(VehicleDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameVehicle>(source.Id.Value)
            : null;

    /// <remarks>
    ///     Only ground vehicles must be processed by this repository.
    ///     Exception is raised otherwise on type disparity after domain object mapping.
    /// </remarks>
    protected override bool IncludeSourceModel(VehicleDTO sourceModel)
        => sourceModel is { Is_spaceship: 0, Is_ground_vehicle: 1 };
}
