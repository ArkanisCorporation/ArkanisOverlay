namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Common.Abstractions.Services;
using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Polly;
using Services;

internal class UexMoonSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexMoonSyncRepository> cacheProvider,
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexMoonSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<UniverseMoonDTO, GameMoon>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver
            .DependsOn<GamePlanet>(this)
            .AlsoDependsOn<GameStarSystem>();

    protected override double CacheTimeFactor
        => 7;

    protected override async Task<UexApiResponse<ICollection<UniverseMoonDTO>>> GetInternalResponseAsync(
        ResiliencePipeline pipeline,
        CancellationToken cancellationToken
    )
    {
        var response = await pipeline.ExecuteAsync(
            async ct => await gameApi.GetMoonsAsync(cancellationToken: ct).ConfigureAwait(false),
            cancellationToken
        );
        return CreateResponse(response, response.Result.Data?.Where(x => x.Is_available > 0).ToList());
    }

    protected override UexApiGameEntityId? GetSourceApiId(UniverseMoonDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
