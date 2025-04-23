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
    IGameEntityRepository<GameProductCategory> itemCategoryRepository,
    IUexItemsApi itemsApi,
    UexGameDataStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexItemRepository> logger
) : UexGameEntityRepositoryBase<ItemDTO, GameItem>(stateProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameProductCategory>();

    protected override async Task<UexApiResponse<ICollection<ItemDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var categories = await itemCategoryRepository.GetAllAsync(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
        var items = new List<ItemDTO>();

        UexApiResponse<GetItemsOkResponse>? response = null;
        var responseDetectedAsNull = false;
        foreach (var category in categories.Where(x => x.CategoryType == GameItemCategoryType.Item))
        {
            var categoryEntityId = category.Id as UexApiGameEntityId;
            var categoryId = (categoryEntityId?.Identity ?? 0).ToString(CultureInfo.InvariantCulture);
            response = await itemsApi.GetItemsByCategoryAsync(categoryId, cancellationToken).ConfigureAwait(false);
            responseDetectedAsNull |= response.Result.Data is null;
            items.AddRange(response.Result.Data ?? []);
        }

        if (items.Count == 0 && responseDetectedAsNull)
        {
            // temporary workaround, some responses return Result.Data=null
            ThrowCouldNotParseResponse();
        }

        return CreateResponse(response, items);
    }

    protected override double? GetSourceApiId(ItemDTO source)
        => source.Id;

    /// <remarks>
    ///     Some items do not have company ID defined.
    ///     Typical example are centurion/imperator subscriber items.
    /// </remarks>
    protected override bool IncludeSourceModel(ItemDTO sourceModel)
        => sourceModel.Id_company > 0;
}
