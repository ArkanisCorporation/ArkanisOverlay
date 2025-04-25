namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;

internal class UexCompanySyncRepository(
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    IExternalSyncCacheProvider<UexCompanySyncRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexCompanySyncRepository> logger
) : UexGameEntitySyncRepositoryBase<CompanyDTO, GameCompany>(stateProvider, cacheProvider, mapper, logger)
{
    protected override double CacheTimeFactor
        => 7;

    protected override async Task<UexApiResponse<ICollection<CompanyDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetCompaniesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(CompanyDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameCompany>(source.Id.Value)
            : null;
}
