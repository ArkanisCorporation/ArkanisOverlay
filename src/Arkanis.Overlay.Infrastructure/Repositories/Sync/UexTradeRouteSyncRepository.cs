namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using System.Collections.Concurrent;
using System.Globalization;
using Common.Abstractions.Services;
using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using MoreAsyncLINQ;
using Services;

internal class UexTradeRouteSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexTradeRouteSyncRepository> cacheProvider,
    IGameEntityRepository<GameCommodity> commodityRepository,
    IUexCommoditiesApi commoditiesApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexTradeRouteSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<CommodityRouteDTO, GameTradeRoute>(stateProvider, cacheProvider, mapper, logger)
{
    private const int BatchSize = 6;

    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameTerminal>(this)
            .AlsoDependsOn<GameCommodity>();

    protected override async Task<UexApiResponse<ICollection<CommodityRouteDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var commodities = commodityRepository.GetAllAsync(cancellationToken)
            .Where(x => !x.IsHarvestable)
            .Where(x => x.IsSellable);

        var items = new ConcurrentBag<CommodityRouteDTO>();
        UexApiResponse<GetCommoditiesRoutesOkResponse>? response = null;

        await foreach (var commodityBatch in commodities.Batch(BatchSize).WithCancellation(cancellationToken))
        {
            await Task.WhenAll(commodityBatch.Select(LoadForPlanetOrbitAsync));
        }

        return CreateResponse(response, items.ToArray());

        async Task<UexApiResponse<GetCommoditiesRoutesOkResponse>> LoadForPlanetOrbitAsync(GameCommodity commodity)
        {
            var commodityEntityId = commodity.Id;
            var commodityId = commodityEntityId.Identity.ToString(CultureInfo.InvariantCulture);
            response = await commoditiesApi.GetCommoditiesRoutesByCommodityAsync(commodityId, cancellationToken).ConfigureAwait(false);
            foreach (var dto in response.Result.Data ?? ThrowCouldNotParseResponse())
            {
                items.Add(dto);
            }

            return response;
        }
    }

    protected override bool IncludeSourceModel(CommodityRouteDTO sourceModel)
        => sourceModel is { Price_origin: > 0, Price_destination: > 0, Id_terminal_origin: > 0, Id_terminal_destination: > 0 };

    protected override IEnumerable<CommodityRouteDTO> FilterSourceModels(IEnumerable<CommodityRouteDTO> models)
        => models
            .OrderByDescending(x => x.Date_added)
            .GroupBy(x => (x.Id_commodity, x.Id_terminal_origin, x.Id_terminal_destination))
            .Select(x => x.First());

    protected override UexApiGameEntityId? GetSourceApiId(CommodityRouteDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
