namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using System.Collections.Concurrent;
using System.Globalization;
using Common.Abstractions.Services;
using Data.Mappers;
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

        // this must be a thread-safe collection due to the batching that follows
        var items = new ConcurrentBag<ItemAttributeDTO>();
        UexApiResponse<GetItemsAttributesOkResponse>? response = null;

        await foreach (var categoryBatch in categories.Batch(BatchSize).WithCancellation(cancellationToken))
        {
            await Task.WhenAll(categoryBatch.Select(LoadForCategoryAsync));
        }

        return CreateResponse(response, items.ToArray());

        async Task<UexApiResponse<GetItemsAttributesOkResponse>> LoadForCategoryAsync(GameProductCategory category)
        {
            var categoryEntityId = category.Id;
            var categoryId = categoryEntityId.Identity.ToString(CultureInfo.InvariantCulture);
            response = await itemsApi.GetItemsAttributesByCategoryAsync(categoryId, cancellationToken).ConfigureAwait(false);
            foreach (var dto in response.Result.Data ?? ThrowCouldNotParseResponse())
            {
                items.Add(dto);
            }

            return response;
        }
    }

    protected override UexApiGameEntityId? GetSourceApiId(ItemAttributeDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
