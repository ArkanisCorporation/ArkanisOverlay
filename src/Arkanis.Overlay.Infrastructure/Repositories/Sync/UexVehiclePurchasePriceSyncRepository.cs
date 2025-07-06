namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Common.Abstractions.Services;
using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexVehiclePurchasePriceSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexVehiclePurchasePriceSyncRepository> cacheProvider,
    IUexVehiclesApi vehiclesApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexVehiclePurchasePriceSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<VehiclePurchasePriceBriefDTO, GameEntityPurchasePrice>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameTerminal>(this);

    protected override async Task<UexApiResponse<ICollection<VehiclePurchasePriceBriefDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await vehiclesApi.GetVehiclesPurchasesPricesAllAsync(cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(VehiclePurchasePriceBriefDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;

    /// <remarks>
    ///     Only process prices which have a non-zero value.
    /// </remarks>
    protected override bool IncludeSourceModel(VehiclePurchasePriceBriefDTO sourceModel)
        => sourceModel is { Price_buy: > 0 };
}
