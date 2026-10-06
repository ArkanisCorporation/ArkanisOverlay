namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Polly;

internal class UexCommoditySyncRepository(
    IUexCommoditiesApi commoditiesApi,
    UexServiceStateProvider stateProvider,
    IExternalSyncCacheProvider<UexCommoditySyncRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexCommoditySyncRepository> logger
) : UexGameEntitySyncRepositoryBase<CommodityDTO, GameCommodity>(stateProvider, cacheProvider, mapper, logger)
{
    protected override async Task<UexApiResponse<ICollection<CommodityDTO>>> GetInternalResponseAsync(
        ResiliencePipeline pipeline,
        CancellationToken cancellationToken
    )
    {
        var response = await pipeline.ExecuteAsync(
            async ct => await commoditiesApi.GetCommoditiesAsync(cancellationToken: ct).ConfigureAwait(false),
            cancellationToken
        );
        return CreateResponse(response, response.Result.Data?.Where(x => x.Is_available > 0).ToList());
    }

    protected override UexApiGameEntityId? GetSourceApiId(CommodityDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
