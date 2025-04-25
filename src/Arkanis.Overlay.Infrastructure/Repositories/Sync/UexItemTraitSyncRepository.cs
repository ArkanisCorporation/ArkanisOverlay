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

internal class UexItemTraitSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexItemTraitSyncRepository> cacheProvider,
    IGameEntityRepository<GameProductCategory> itemCategoryRepository,
    IUexItemsApi itemsApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexItemTraitSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<ItemAttributeDTO, GameItemTrait>(stateProvider, cacheProvider, mapper, logger)
{
    private const int BatchSize = 4;

    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameProductCategory>(this);

    protected override async Task<UexApiResponse<ICollection<ItemAttributeDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var categories = itemCategoryRepository.GetAllAsync(cancellationToken)
            .Where(x => x.CategoryType == GameItemCategoryType.Item)
            .Where(category => category.Id.Identity > 0);

        var items = new List<ItemAttributeDTO>();
        UexApiResponse<GetItemsAttributesOkResponse>? response = null;

        await foreach (var categoryBatch in categories.Batch(BatchSize).WithCancellation(cancellationToken))
        {
            await Task.WhenAll(categoryBatch.Select(LoadForCategoryAsync));
        }

        return CreateResponse(response, items);

        async Task<UexApiResponse<GetItemsAttributesOkResponse>> LoadForCategoryAsync(GameProductCategory category)
        {
            var categoryEntityId = category.Id;
            var categoryId = categoryEntityId.Identity.ToString(CultureInfo.InvariantCulture);
            response = await itemsApi.GetItemsAttributesByCategoryAsync(categoryId, cancellationToken).ConfigureAwait(false);
            items.AddRange(response.Result.Data ?? ThrowCouldNotParseResponse());
            return response;
        }
    }

    protected override UexApiGameEntityId? GetSourceApiId(ItemAttributeDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameItemTrait>(source.Id.Value)
            : null;
}
