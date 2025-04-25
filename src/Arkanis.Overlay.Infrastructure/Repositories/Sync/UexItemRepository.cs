namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using System.Globalization;
using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Enums;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexItemRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexItemRepository> cacheProvider,
    IGameEntityRepository<GameProductCategory> itemCategoryRepository,
    IUexItemsApi itemsApi,
    UexGameDataStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexItemRepository> logger
) : UexGameEntityRepositoryBase<ItemDTO, GameItem>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameProductCategory>(this);

    protected override async Task<UexApiResponse<ICollection<ItemDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var categories = itemCategoryRepository.GetAllAsync(cancellationToken)
            .Where(x => x.CategoryType == GameItemCategoryType.Item)
            .Where(category => category.Id.Identity > 0);

        var items = new List<ItemDTO>();
        var responseDetectedAsNull = false;
        UexApiResponse<GetItemsOkResponse>? response = null;

        await foreach (var category in categories)
        {
            var categoryEntityId = category.Id;
            var categoryId = (categoryEntityId?.Identity ?? 0).ToString(CultureInfo.InvariantCulture);
            response = await itemsApi.GetItemsByCategoryAsync(categoryId, cancellationToken).ConfigureAwait(false);
            responseDetectedAsNull |= response.Result.Data is null;
            items.AddRange(response.Result.Data ?? []);
#if DEBUG
            break;
#endif
        }

        if (items.Count == 0 && responseDetectedAsNull)
        {
            // temporary workaround, some responses return Result.Data=null
            ThrowCouldNotParseResponse();
        }

        return CreateResponse(response, items);
    }

    protected override UexApiGameEntityId? GetSourceApiId(ItemDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameItem>(source.Id.Value)
            : null;

    /// <remarks>
    ///     Some items do not have company ID defined.
    ///     Typical example are centurion/imperator subscriber items.
    /// </remarks>
    protected override bool IncludeSourceModel(ItemDTO sourceModel)
        => sourceModel.Id_company > 0;
}
