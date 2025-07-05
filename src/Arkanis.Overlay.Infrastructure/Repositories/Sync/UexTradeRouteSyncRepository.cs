namespace Arkanis.Overlay.Infrastructure.Repositories.Sync;

using System.Collections.Concurrent;
using System.Globalization;
using Data.Mappers;
using Domain.Abstractions;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using External.UEX.Abstractions;
using Local;
using Microsoft.Extensions.Logging;
using Services;

internal class UexTradeRouteSyncRepository(
    GameEntityRepositoryDependencyResolver dependencyResolver,
    IExternalSyncCacheProvider<UexTradeRouteSyncRepository> cacheProvider,
    IGameEntityRepository<GamePlanet> planetRepository,
    IUexCommoditiesApi commoditiesApi,
    UexServiceStateProvider stateProvider,
    UexApiDtoMapper mapper,
    ILogger<UexTradeRouteSyncRepository> logger
) : UexGameEntitySyncRepositoryBase<CommodityRouteDTO, GameTradeRoute>(stateProvider, cacheProvider, mapper, logger)
{
    protected override IDependable GetDependencies()
        => dependencyResolver.DependsOn<GameTerminal>(this)
            .AlsoDependsOn<GameCommodity>();

    protected override async Task<UexApiResponse<ICollection<CommodityRouteDTO>>> GetInternalResponseAsync(CancellationToken cancellationToken)
    {
        var planets = planetRepository.GetAllAsync(cancellationToken);

        var items = new ConcurrentBag<CommodityRouteDTO>();
        UexApiResponse<GetCommoditiesRoutesOkResponse>? response = null;

        await foreach (var planet in planets)
        {
            await LoadForPlanetOrbitAsync(planet);
        }

        return CreateResponse(response, items.ToArray());

        async Task<UexApiResponse<GetCommoditiesRoutesOkResponse>> LoadForPlanetOrbitAsync(GamePlanet planet)
        {
            var orbitEntityId = planet.Id;
            var orbitId = orbitEntityId.Identity.ToString(CultureInfo.InvariantCulture);
            response = await commoditiesApi.GetCommoditiesRoutesByOrbitOriginAsync(orbitId, cancellationToken).ConfigureAwait(false);
            foreach (var dto in response.Result.Data ?? ThrowCouldNotParseResponse())
            {
                items.Add(dto);
            }

            return response;
        }
    }

    protected override bool IncludeSourceModel(CommodityRouteDTO sourceModel)
        => sourceModel is { Price_origin: > 0, Price_destination: > 0, Id_terminal_origin: > 0, Id_terminal_destination: > 0 };

    protected override IEnumerable<CommodityRouteDTO> FilterSourceModels(IEnumerable<CommodityRouteDTO> models)
        => models
            .OrderByDescending(x => x.Date_added)
            .GroupBy(x => (x.Id_commodity, x.Id_terminal_origin, x.Id_terminal_destination))
            .Select(x => x.First());

    protected override UexApiGameEntityId? GetSourceApiId(CommodityRouteDTO source)
        => source.Id is not null
            ? Mapper.CreateGameEntityId(source, x => x.Id)
            : null;
}
