namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Common.Abstractions.Services;
using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexCommodityPriceSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IUexCommoditiesApi commoditiesApi,
    UexServiceStateProvider stateProvider,
    IExternalSyncCacheProvider<UexCommodityPriceSyncRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexCommodityPriceSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<CommodityPriceBriefDTO, GameEntityTradePrice>(stateProvider, cacheProvider, mapper, logger)
{
    protected override double CacheTimeFactor
        => 0.5;

    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameTerminal>(this);

    protected override async Task<UexApiResponse<ICollection<CommodityPriceBriefDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await commoditiesApi.GetCommoditiesPricesAllAsync(cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(CommodityPriceBriefDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;

    /// <remarks>
    ///     Only process prices which have a non-zero value.
    /// </remarks>
    protected override bool IncludeSourceModel(CommodityPriceBriefDTO sourceModel)
        => sourceModel is { Price_buy: > 0 } or { Price_sell: > 0 };
}
