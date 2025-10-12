namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Polly;

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

    protected override async Task<UexApiResponse<ICollection<CompanyDTO>>> GetInternalResponseAsync(
        ResiliencePipeline pipeline,
        CancellationToken cancellationToken
    )
    {
        var response = await pipeline.ExecuteAsync(
            async ct => await gameApi.GetCompaniesAsync(cancellationToken: ct).ConfigureAwait(false),
            cancellationToken
        );
        return CreateResponse(response, response.Result.Data);
    }

    protected override UexApiGameEntityId? GetSourceApiId(CompanyDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
