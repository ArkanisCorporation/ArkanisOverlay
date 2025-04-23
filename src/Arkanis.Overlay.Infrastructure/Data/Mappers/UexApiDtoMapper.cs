namespace Arkanis.Overlay.Infrastructure.Data.Mappers;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Domain.Models.Game;
using Exceptions;
using External.UEX.Abstractions;
using Riok.Mapperly.Abstractions;
using Services.Abstractions;

[Mapper(
    RequiredMappingStrategy = RequiredMappingStrategy.Target,
    EnumNamingStrategy = EnumNamingStrategy.SnakeCase,
    EnumMappingIgnoreCase = true
)]
[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
internal partial class UexApiDtoMapper(IGameEntityHydrationService hydrationService)
{
    internal readonly ConcurrentDictionary<string, GameEntity> CachedGameEntities = [];

    public async ValueTask<GameEntity> ToGameEntityAsync<TSource>(TSource source)
        => source switch
        {
            UniverseStarSystemDTO system => await ToGameEntityAsync(system),
            UniversePlanetDTO planet => await ToGameEntityAsync(planet),
            UniverseMoonDTO moon => await ToGameEntityAsync(moon),
            UniverseSpaceStationDTO spaceStation => await ToGameEntityAsync(spaceStation),
            UniverseCityDTO city => await ToGameEntityAsync(city),
            UniverseOutpostDTO outpost => await ToGameEntityAsync(outpost),
            UniverseTerminalDTO terminal => await ToGameEntityAsync(terminal),
            CommodityDTO commodity => await ToGameEntityAsync(commodity),
            ItemDTO item => await ToGameEntityAsync(item),
            CompanyDTO company => await ToGameEntityAsync(company),
            CategoryDTO category => await ToGameEntityAsync(category),
            VehicleDTO vehicle => await ToGameEntityAsync(vehicle),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Cannot map to game entity from unsupported source type."),
        };

    [UserMapping(Default = true)]
    public async ValueTask<GameStarSystem> ToGameEntityAsync(UniverseStarSystemDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameStarSystem>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    public async ValueTask<GamePlanet> ToGameEntityAsync(UniversePlanetDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GamePlanet>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    public async ValueTask<GameMoon> ToGameEntityAsync(UniverseMoonDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameMoon>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    public async ValueTask<GameCity> ToGameEntityAsync(UniverseCityDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameCity>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    public async ValueTask<GameSpaceStation> ToGameEntityAsync(UniverseSpaceStationDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameSpaceStation>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    public async ValueTask<GameOutpost> ToGameEntityAsync(UniverseOutpostDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameOutpost>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    public async ValueTask<GameTerminal> ToGameEntityAsync(UniverseTerminalDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameTerminal>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    public async ValueTask<GameCommodity> ToGameEntityAsync(CommodityDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameCommodity>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    public async ValueTask<GameItem> ToGameEntityAsync(ItemDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameItem>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    public async ValueTask<GameCompany> ToGameEntityAsync(CompanyDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameCompany>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    public async ValueTask<GameProductCategory> ToGameEntityAsync(CategoryDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameProductCategory>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    public async ValueTask<GameVehicle> ToGameEntityAsync(VehicleDTO source)
    {
        GameVehicle result = source switch
        {
            { Is_spaceship: 1 } => MapInternalSpaceShip(source),
            { Is_ground_vehicle: 1 } => MapInternalGroundVehicle(source),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unable to select corresponding vehicle type."),
        };
        CacheGameEntityId<GameVehicle>(source.Id, result);
        await hydrationService.HydrateAsync(result);
        return result;
    }

    private void CacheGameEntityId<T>(double? sourceId, GameEntity result) where T : GameEntity
        => CachedGameEntities[CreateCacheEntityKey<T>(sourceId)] = result;

    private T? ResolveCachedGameEntity<T>(double? sourceId, [CallerArgumentExpression("sourceId")] string sourceIdExpression = "") where T : GameEntity
    {
        var cacheEntityKey = CreateCacheEntityKey<T>(sourceId);
        var cachedEntity = CachedGameEntities.GetValueOrDefault(cacheEntityKey) as T;
        return sourceId > 0 && cachedEntity is null
            ? ThrowInvalidCacheException<T>(sourceId, sourceIdExpression)
            : cachedEntity;
    }

    private static string CreateCacheEntityKey<T>(double? sourceId)
        => $"{typeof(T).Name}-{sourceId}";

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(UniverseStarSystemDTO.Name), "fullName")]
    [MapProperty(nameof(UniverseStarSystemDTO.Code), "codeName")]
    private partial GameStarSystem MapInternal(UniverseStarSystemDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(UniversePlanetDTO.Name), "fullName")]
    [MapProperty(nameof(UniversePlanetDTO.Code), "codeName")]
    [MapPropertyFromSource("location", Use = nameof(GetGameLocationForPlanet))]
    private partial GamePlanet MapInternal(UniversePlanetDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(UniverseMoonDTO.Name), "fullName")]
    [MapProperty(nameof(UniverseMoonDTO.Code), "codeName")]
    [MapPropertyFromSource("location", Use = nameof(GetGameLocationForMoon))]
    private partial GameMoon MapInternal(UniverseMoonDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(UniverseSpaceStationDTO.Name), "fullName")]
    [MapProperty(nameof(UniverseSpaceStationDTO.Nickname), "shortName")]
    [MapPropertyFromSource("location", Use = nameof(GetGameLocationForSpaceStation))]
    private partial GameSpaceStation MapInternal(UniverseSpaceStationDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(UniverseCityDTO.Name), "fullName")]
    [MapProperty(nameof(UniverseCityDTO.Code), "codeName")]
    [MapPropertyFromSource("location", Use = nameof(GetGameLocationForCity))]
    private partial GameCity MapInternal(UniverseCityDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(UniverseOutpostDTO.Name), "fullName")]
    [MapProperty(nameof(UniverseOutpostDTO.Nickname), "shortName")]
    [MapPropertyFromSource("location", Use = nameof(GetGameLocationForOutpost))]
    private partial GameOutpost MapInternal(UniverseOutpostDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(UniverseTerminalDTO.Name), "fullName")]
    [MapProperty(nameof(UniverseTerminalDTO.Nickname), "shortName")]
    [MapProperty(nameof(UniverseTerminalDTO.Code), "codeName")]
    [MapPropertyFromSource("location", Use = nameof(GetGameLocationForTerminal))]
    private partial GameTerminal MapInternal(UniverseTerminalDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(CommodityDTO.Name), "fullName")]
    [MapProperty(nameof(CommodityDTO.Code), "codeName")]
    private partial GameCommodity MapInternal(CommodityDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(ItemDTO.Name), "fullName")]
    [MapPropertyFromSource("manufacturer", Use = nameof(GetCompanyForItem))]
    [MapPropertyFromSource("category", Use = nameof(GetCategoryForItem))]
    private partial GameItem MapInternal(ItemDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(CompanyDTO.Name), "fullName")]
    [MapProperty(nameof(CompanyDTO.Nickname), "shortName")]
    private partial GameCompany MapInternal(CompanyDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(CategoryDTO.Name), "fullName")]
    [MapProperty(nameof(CategoryDTO.Section), "section")]
    [MapProperty(nameof(CategoryDTO.Type), nameof(GameProductCategory.CategoryType))]
    private partial GameProductCategory MapInternal(CategoryDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(VehicleDTO.Name_full), "fullName")]
    [MapProperty(nameof(VehicleDTO.Name), "shortName")]
    [MapPropertyFromSource("manufacturer", Use = nameof(GetCompanyForVehicle))]
    private partial GameSpaceShip MapInternalSpaceShip(VehicleDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(VehicleDTO.Name_full), "fullName")]
    [MapProperty(nameof(VehicleDTO.Name), "shortName")]
    [MapPropertyFromSource("manufacturer", Use = nameof(GetCompanyForVehicle))]
    private partial GameGroundVehicle MapInternalGroundVehicle(VehicleDTO source);

    [DoesNotReturn]
    private static T ThrowInvalidCacheException<T>(double? sourceId, [CallerArgumentExpression("sourceId")] string sourceIdExpression = "")
        => throw new ObjectMappingMissingDependentObjectException(
            $"Could not resolve cached entity instance of {typeof(T)} for: {sourceIdExpression} == {sourceId}"
        );

    [DoesNotReturn]
    private static TTarget ThrowMissingMappingException<TTarget, TSource>(double? sourceId)
        => throw new ObjectMappingException($"Could not find correct mapping for {typeof(TTarget)} in {typeof(TSource)} with ID {sourceId}.", null);

    private GameLocationEntity GetGameLocationForPlanet(UniversePlanetDTO planet)
        => ResolveCachedGameEntity<GameStarSystem>(planet.Id_star_system)
           ?? ThrowMissingMappingException<GameLocationEntity, UniversePlanetDTO>(planet.Id);

    private GameLocationEntity GetGameLocationForMoon(UniverseMoonDTO moon)
        => ResolveCachedGameEntity<GamePlanet>(moon.Id_planet) as GameLocationEntity
           ?? ResolveCachedGameEntity<GameStarSystem>(moon.Id_star_system)
           ?? ThrowMissingMappingException<GameLocationEntity, UniverseMoonDTO>(moon.Id);

    private GameLocationEntity GetGameLocationForSpaceStation(UniverseSpaceStationDTO spaceStation)
        => ResolveCachedGameEntity<GameCity>(spaceStation.Id_city) as GameLocationEntity
           ?? ResolveCachedGameEntity<GamePlanet>(spaceStation.Id_planet) as GameLocationEntity
           ?? ResolveCachedGameEntity<GameMoon>(spaceStation.Id_moon) as GameLocationEntity
           ?? ResolveCachedGameEntity<GameStarSystem>(spaceStation.Id_star_system)
           ?? ThrowMissingMappingException<GameLocationEntity, UniverseSpaceStationDTO>(spaceStation.Id);

    private GameLocationEntity GetGameLocationForCity(UniverseCityDTO city)
        => ResolveCachedGameEntity<GamePlanet>(city.Id_planet) as GameLocationEntity
           ?? ResolveCachedGameEntity<GameMoon>(city.Id_moon)
           ?? ThrowMissingMappingException<GameLocationEntity, UniverseCityDTO>(city.Id);

    private GameLocationEntity GetGameLocationForOutpost(UniverseOutpostDTO outpost)
        => ResolveCachedGameEntity<GamePlanet>(outpost.Id_planet) as GameLocationEntity
           ?? ResolveCachedGameEntity<GameMoon>(outpost.Id_moon)
           ?? ThrowMissingMappingException<GameLocationEntity, UniverseOutpostDTO>(outpost.Id);

    private GameLocationEntity GetGameLocationForTerminal(UniverseTerminalDTO terminal)
        => ResolveCachedGameEntity<GameCity>(terminal.Id_city) as GameLocationEntity
           ?? ResolveCachedGameEntity<GameOutpost>(terminal.Id_outpost) as GameLocationEntity
           ?? ResolveCachedGameEntity<GameSpaceStation>(terminal.Id_space_station) as GameLocationEntity
           ?? ResolveCachedGameEntity<GameStarSystem>(terminal.Id_star_system)
           ?? ThrowMissingMappingException<GameLocationEntity, UniverseTerminalDTO>(terminal.Id);

    private GameCompany GetCompanyForItem(ItemDTO item)
        => ResolveCachedGameEntity<GameCompany>(item.Id_company)
           ?? ThrowMissingMappingException<GameCompany, ItemDTO>(item.Id);

    private GameCompany GetCompanyForVehicle(VehicleDTO vehicle)
        => ResolveCachedGameEntity<GameCompany>(vehicle.Id_company)
           ?? ThrowMissingMappingException<GameCompany, VehicleDTO>(vehicle.Id);

    private GameProductCategory GetCategoryForItem(ItemDTO item)
        => ResolveCachedGameEntity<GameProductCategory>(item.Id_category)
           ?? ThrowMissingMappingException<GameProductCategory, ItemDTO>(item.Id);
}
