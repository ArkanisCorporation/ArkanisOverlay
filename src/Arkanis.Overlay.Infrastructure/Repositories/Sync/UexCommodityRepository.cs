namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;

internal class UexCommodityRepository(
    IUexCommoditiesApi commoditiesApi,
    UexGameDataStateProvider stateProvider,
    IExternalSyncCacheProvider<UexCommodityRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexCommodityRepository> logger
) : UexGameEntityRepositoryBase<CommodityDTO, GameCommodity>(stateProvider, cacheProvider, mapper, logger)
{
    protected override async Task<UexApiResponse<ICollection<CommodityDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await commoditiesApi.GetCommoditiesAsync(cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data?.Where(x => x.Is_available > 0).ToList());
    }

    protected override UexApiGameEntityId? GetSourceApiId(CommodityDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameCommodity>(source.Id.Value)
            : null;
}
