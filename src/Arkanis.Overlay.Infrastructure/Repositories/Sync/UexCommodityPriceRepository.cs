namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexCommodityPriceRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IUexCommoditiesApi commoditiesApi,
    UexServiceStateProvider stateProvider,
    IExternalSyncCacheProvider<UexCommodityPriceRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexCommodityPriceRepository> logger
) : UexGameEntityRepositoryBase<CommodityPriceBriefDTO, GameCommodityPricing>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameTerminal>(this);

    protected override async Task<UexApiResponse<ICollection<CommodityPriceBriefDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await commoditiesApi.GetCommoditiesPricesAllAsync(cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(CommodityPriceBriefDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameCommodityPricing>(source.Id.Value)
            : null;

    /// <remarks>
    ///     Only process prices which have a non-zero price.
    /// </remarks>
    protected override bool IncludeSourceModel(CommodityPriceBriefDTO sourceModel)
        => sourceModel is { Price_buy: > 0 } or { Price_sell: > 0 };
}
