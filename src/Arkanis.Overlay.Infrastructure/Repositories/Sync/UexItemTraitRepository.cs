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

internal class UexItemTraitRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexItemTraitRepository> cacheProvider,
    IGameEntityRepository<GameProductCategory> itemCategoryRepository,
    IUexItemsApi itemsApi,
    UexGameDataStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexItemTraitRepository> logger
) : UexGameEntityRepositoryBase<ItemAttributeDTO, GameItemTrait>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameProductCategory>(this);

    protected override async Task<UexApiResponse<ICollection<ItemAttributeDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var categories = itemCategoryRepository.GetAllAsync(cancellationToken)
            .Where(x => x.CategoryType == GameItemCategoryType.Item)
            .Where(category => category.Id.Identity > 0);

        var items = new List<ItemAttributeDTO>();
        UexApiResponse<GetItemsAttributesOkResponse>? response = null;
        await foreach (var category in categories)
        {
            var categoryEntityId = category.Id;
            var categoryId = (categoryEntityId?.Identity ?? 0).ToString(CultureInfo.InvariantCulture);
            response = await itemsApi.GetItemsAttributesByCategoryAsync(categoryId, cancellationToken).ConfigureAwait(false);
            items.AddRange(response.Result.Data ?? ThrowCouldNotParseResponse());
#if DEBUG
            break;
#endif
        }

        return CreateResponse(response, items);
    }

    protected override UexApiGameEntityId? GetSourceApiId(ItemAttributeDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameItemTrait>(source.Id.Value)
            : null;
}
