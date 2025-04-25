namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexVehicleRentPriceSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexVehicleRentPriceSyncRepository> cacheProvider,
    IUexVehiclesApi vehiclesApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexVehicleRentPriceSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<VehicleRentalPriceBriefDTO, GameVehicleRentalPricing>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameTerminal>(this);

    protected override async Task<UexApiResponse<ICollection<VehicleRentalPriceBriefDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await vehiclesApi.GetVehiclesRentalsPricesAllAsync(cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(VehicleRentalPriceBriefDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameCommodityPricing>(source.Id.Value)
            : null;

    /// <remarks>
    ///     Only process prices which have a non-zero price.
    /// </remarks>
    protected override bool IncludeSourceModel(VehicleRentalPriceBriefDTO sourceModel)
        => sourceModel is { Price_rent: > 0 };
}
