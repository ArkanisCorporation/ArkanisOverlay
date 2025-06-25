namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexTerminalSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexTerminalSyncRepository> cacheProvider,
    IUexGameApi gameApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexTerminalSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<UniverseTerminalDTO, GameTerminal>(stateProvider, cacheProvider, mapper, logger)
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

    protected override bool IncludeSourceModel(UniverseTerminalDTO sourceModel)
        => sourceModel.Is_player_owned == 0;

    protected override UexApiGameEntityId? GetSourceApiId(UniverseTerminalDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
