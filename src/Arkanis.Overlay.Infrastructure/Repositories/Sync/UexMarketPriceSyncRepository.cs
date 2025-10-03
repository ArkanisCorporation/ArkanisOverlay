namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Polly;

internal class UexMarketPriceSyncRepository(
    IExternalSyncCacheProvider<UexMarketPriceSyncRepository> cacheProvider,
    IUexMarketplaceApi marketplaceApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexMarketPriceSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<MarketplaceListingDTO, GameEntityMarketPrice>(stateProvider, cacheProvider, mapper, logger)
{
    protected override async Task<UexApiResponse<ICollection<MarketplaceListingDTO>>> GetInternalResponseAsync(
        ResiliencePipeline pipeline,
        CancellationToken cancellationToken
    )
    {
        var response = await pipeline.ExecuteAsync(
            async ct => await marketplaceApi.GetMarketplaceListingsAsync(cancellationToken: ct).ConfigureAwait(false),
            cancellationToken
        );
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(MarketplaceListingDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;

    /// <remarks>
    ///     Only process prices which have a non-zero value and a valid item id.
    /// </remarks>
    protected override bool IncludeSourceModel(MarketplaceListingDTO sourceModel)
        => sourceModel is { Id_item: > 0, Price: > 0 };
}
