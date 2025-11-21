namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using System.Collections.Concurrent;
using System.Globalization;
using Common.Extensions;
using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Enums;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Polly;
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
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameProductCategory>(this);

    protected override async Task<UexApiResponse<ICollection<ItemAttributeDTO>>> GetInternalResponseAsync(
        ResiliencePipeline pipeline,
        CancellationToken cancellationToken
    )
    {
        var categories = itemCategoryRepository.GetAllAsync(cancellationToken)
            .Where(x => x.CategoryType == GameItemCategoryType.Item)
            .Where(category => category.Id.Identity > 0);

        // this must be a thread-safe collection due to the batching that follows
        var items = new ConcurrentBag<ItemAttributeDTO>();
        UexApiResponse<GetItemsAttributesOkResponse>? response = null;

        await foreach (var categoryBatch in categories.Batch(UexSharedResiliency.ApiRequestBatchSize, cancellationToken))
        {
            await Task.WhenAll(categoryBatch.Select(LoadForCategoryAsync));
        }

        return CreateResponse(response, items.ToArray());

        async Task<UexApiResponse<GetItemsAttributesOkResponse>> LoadForCategoryAsync(GameProductCategory category)
        {
            var categoryEntityId = category.Id;
            var categoryId = categoryEntityId.Identity.ToString(CultureInfo.InvariantCulture);
            response = await pipeline.ExecuteAsync(
                async ct => await itemsApi.GetItemsAttributesByCategoryAsync(categoryId, ct).ConfigureAwait(false),
                cancellationToken
            );
            foreach (var dto in response.Result.Data ?? ThrowCouldNotParseResponse())
            {
                items.Add(dto);
            }

            return response;
        }
    }

    protected override bool IncludeSourceModel(ItemAttributeDTO sourceModel)
        => sourceModel is { Value: not null };

    protected override UexApiGameEntityId? GetSourceApiId(ItemAttributeDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
