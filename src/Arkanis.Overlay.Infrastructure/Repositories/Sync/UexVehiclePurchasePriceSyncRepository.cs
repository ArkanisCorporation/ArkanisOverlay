namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
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
) : UexGameEntitySyncRepositoryBase<VehiclePurchasePriceBriefDTO, GameVehiclePurchasePricing>(stateProvider, cacheProvider, mapper, logger)
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
            ? UexApiGameEntityId.Create<GameCommodityPricing>(source.Id.Value)
            : null;

    /// <remarks>
    ///     Only process prices which have a non-zero price.
    /// </remarks>
    protected override bool IncludeSourceModel(VehiclePurchasePriceBriefDTO sourceModel)
        => sourceModel is { Price_buy: > 0 };
}
