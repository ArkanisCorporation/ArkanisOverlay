namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;

internal class UexStarSystemSyncRepository(
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    IExternalSyncCacheProvider<UexStarSystemSyncRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexStarSystemSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<UniverseStarSystemDTO, GameStarSystem>(stateProvider, cacheProvider, mapper, logger)
{
    protected override async Task<UexApiResponse<ICollection<UniverseStarSystemDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetStarSystemsAsync(cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data?.Where(x => x.Is_available > 0).ToList());
    }

    protected override UexApiGameEntityId? GetSourceApiId(UniverseStarSystemDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameStarSystem>(source.Id.Value)
            : null;
}
