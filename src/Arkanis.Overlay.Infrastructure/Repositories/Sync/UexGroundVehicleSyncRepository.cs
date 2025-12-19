namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Polly;
using Services;

internal class UexGroundVehicleSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    IExternalSyncCacheProvider<UexGroundVehicleSyncRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexGroundVehicleSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<VehicleDTO, GameGroundVehicle>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameCompany>(this);

    protected override async Task<UexApiResponse<ICollection<VehicleDTO>>> GetInternalResponseAsync(
        ResiliencePipeline pipeline,
        CancellationToken cancellationToken
    )
    {
        var response = await pipeline.ExecuteAsync(
            async ct => await gameApi.GetVehiclesAsync(cancellationToken: ct).ConfigureAwait(false),
            cancellationToken
        );
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(VehicleDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;

    /// <remarks>
    ///     Only ground vehicles must be processed by this repository.
    ///     Exception is raised otherwise on type disparity after domain object mapping.
    /// </remarks>
    protected override bool IncludeSourceModel(VehicleDTO sourceModel)
        => sourceModel is { Is_ground_vehicle: 1 };
}
