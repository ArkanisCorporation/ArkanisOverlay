namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexItemPriceSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IUexItemsApi itemsApi,
    UexServiceStateProvider stateProvider,
    IExternalSyncCacheProvider<UexItemPriceSyncRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexItemPriceSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<ItemPriceBriefDTO, GameEntityTradePrice>(stateProvider, cacheProvider, mapper, logger)
{
    protected override double CacheTimeFactor
        => 0.5;

    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameTerminal>(this);

    protected override async Task<UexApiResponse<ICollection<ItemPriceBriefDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await itemsApi.GetItemsPricesAllAsync(cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(ItemPriceBriefDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;

    /// <remarks>
    ///     Only process prices which have a non-zero value.
    /// </remarks>
    protected override bool IncludeSourceModel(ItemPriceBriefDTO sourceModel)
        => sourceModel is { Price_buy: > 0 } or { Price_sell: > 0 };
}
