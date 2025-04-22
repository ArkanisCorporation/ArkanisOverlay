namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;

internal class UexCompanyRepository(
    IUexGameApi gameApi,
    UexGameDataStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexCompanyRepository> logger
) : UexGameEntityRepositoryBase<CompanyDTO, GameCompany>(stateProvider, mapper, logger)
{
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
