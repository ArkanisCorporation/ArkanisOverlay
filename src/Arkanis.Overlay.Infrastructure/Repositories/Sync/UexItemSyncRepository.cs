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
using MoreAsyncLINQ;
using Services;

internal class UexItemSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexItemSyncRepository> cacheProvider,
    IGameEntityRepository<GameProductCategory> itemCategoryRepository,
    IUexItemsApi itemsApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexItemSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<ItemDTO, GameItem>(stateProvider, cacheProvider, mapper, logger)
{
    private const int BatchSize = 4;

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

        await foreach (var categoryBatch in categories.Batch(BatchSize).WithCancellation(cancellationToken))
        {
            await Task.WhenAll(categoryBatch.Select(LoadForCategoryAsync));
        }

        if (items.Count == 0 && responseDetectedAsNull)
        {
            // temporary workaround, some responses return Result.Data=null
            ThrowCouldNotParseResponse();
        }

        return CreateResponse(response, items);

        async Task LoadForCategoryAsync(GameProductCategory category)
        {
            var categoryEntityId = category.Id;
            var categoryId = categoryEntityId.Identity.ToString(CultureInfo.InvariantCulture);
            response = await itemsApi.GetItemsByCategoryAsync(categoryId, cancellationToken).ConfigureAwait(false);
            responseDetectedAsNull |= response.Result.Data is null;
            items.AddRange(response.Result.Data ?? []);
        }
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
