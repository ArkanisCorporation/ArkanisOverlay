namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexTerminalRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexTerminalRepository> cacheProvider,
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexTerminalRepository> logger
) : UexGameEntityRepositoryBase<UniverseTerminalDTO, GameTerminal>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver
            .DependsOn<GameCity>(this)
            .AlsoDependsOn<GameOutpost>()
            .AlsoDependsOn<GameSpaceStation>();

    protected override async Task<UexApiResponse<ICollection<UniverseTerminalDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var response = await gameApi.GetTerminalsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return CreateResponse(response, response.Result.Data?.Where(x => x.Is_available > 0).ToList());
    }

    protected override UexApiGameEntityId? GetSourceApiId(UniverseTerminalDTO source)
        => source.Id is not null
            ? UexApiGameEntityId.Create<GameTerminal>(source.Id.Value)
            : null;
}
