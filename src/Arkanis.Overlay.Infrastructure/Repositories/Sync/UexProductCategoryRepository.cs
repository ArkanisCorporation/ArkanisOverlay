namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;

internal class UexProductCategoryRepository(
    IUexGameApi gameApi,
    UexGameDataStateProvider stateProvider,
    IExternalSyncCacheProvider<UexProductCategoryRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexProductCategoryRepository> logger
) : UexGameEntityRepositoryBase<CategoryDTO, GameProductCategory>(stateProvider, cacheProvider, mapper, logger)
{
    protected override async Task<UexApiResponse<ICollection<CategoryDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetCategoriesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(CategoryDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameProductCategory>(source.Id.Value)
            : null;
}
