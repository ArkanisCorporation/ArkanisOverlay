namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Polly;
using Services;

internal class UexCitySyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    IExternalSyncCacheProvider<UexCitySyncRepository> cacheProvider,
    UexApiDtoMapper mapper,
    ILogger<UexCitySyncRepository> logger
) : UexGameEntitySyncRepositoryBase<UniverseCityDTO, GameCity>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver
            .DependsOn<GamePlanet>(this)
            .AlsoDependsOn<GameMoon>();

    protected override double CacheTimeFactor
        => 7;

    protected override async Task<UexApiResponse<ICollection<UniverseCityDTO>>> GetInternalResponseAsync(
        ResiliencePipeline pipeline,
        CancellationToken cancellationToken
    )
    {
        var response = await pipeline.ExecuteAsync(
            async ct => await gameApi.GetCitiesAsync(cancellationToken: ct).ConfigureAwait(false),
            cancellationToken
        );
        return CreateResponse(response, response.Result.Data?.Where(x => x.Is_available > 0).ToList());
    }

    protected override UexApiGameEntityId? GetSourceApiId(UniverseCityDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
