namespace Arkanis.Overlay.Infrastructure.Data.Mappers;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Domain.Abstractions.Game;
using Domain.Enums;
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
    internal readonly ConcurrentDictionary<string, IGameEntity> CachedGameEntities = [];

    public async ValueTask<IGameEntity> ToGameEntityAsync<TSource>(TSource source)
    {
        IGameEntity result = source switch
        {
            UniverseStarSystemDTO system => ToGameEntity(system),
            UniversePlanetDTO planet => ToGameEntity(planet),
            UniverseMoonDTO moon => ToGameEntity(moon),
            UniverseSpaceStationDTO spaceStation => ToGameEntity(spaceStation),
            UniverseCityDTO city => ToGameEntity(city),
            UniverseOutpostDTO outpost => ToGameEntity(outpost),
            UniverseTerminalDTO terminal => ToGameEntity(terminal),
            CommodityDTO commodity => ToGameEntity(commodity),
            ItemDTO item => ToGameEntity(item),
            ItemAttributeDTO itemAttribute => ToGameEntity(itemAttribute),
            CompanyDTO company => ToGameEntity(company),
            CategoryDTO category => ToGameEntity(category),
            VehicleDTO vehicle => ToGameEntity(vehicle),
            ItemPriceBriefDTO itemPrice => ToGameEntity(itemPrice),
            CommodityPriceBriefDTO commodityPrice => ToGameEntity(commodityPrice),
            VehiclePurchasePriceBriefDTO vehiclePurchasePrice => ToGameEntity(vehiclePurchasePrice),
            VehicleRentalPriceBriefDTO vehicleRentalPrice => ToGameEntity(vehicleRentalPrice),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Cannot map to game entity from unsupported source type."),
        };

        await hydrationService.HydrateAsync(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameStarSystem ToGameEntity(UniverseStarSystemDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameStarSystem>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GamePlanet ToGameEntity(UniversePlanetDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GamePlanet>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameMoon ToGameEntity(UniverseMoonDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameMoon>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameCity ToGameEntity(UniverseCityDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameCity>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameSpaceStation ToGameEntity(UniverseSpaceStationDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameSpaceStation>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameOutpost ToGameEntity(UniverseOutpostDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameOutpost>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameTerminal ToGameEntity(UniverseTerminalDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameTerminal>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameCommodity ToGameEntity(CommodityDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameCommodity>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameItem ToGameEntity(ItemDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameItem>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameItemTrait ToGameEntity(ItemAttributeDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameItemTrait>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameCompany ToGameEntity(CompanyDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameCompany>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameProductCategory ToGameEntity(CategoryDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameProductCategory>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameVehicle ToGameEntity(VehicleDTO source)
    {
        GameVehicle result = source switch
        {
            { Is_spaceship: 1 } => MapInternalSpaceShip(source),
            { Is_ground_vehicle: 1 } => MapInternalGroundVehicle(source),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unable to select corresponding vehicle type."),
        };
        CacheGameEntityId<GameVehicle>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameEntityPricing ToGameEntity(CommodityPriceBriefDTO source)
    {
        var purchaseResult = MapInternalPurchasePrice(source);
        CacheGameEntityId<GameCommodityPricing>(source.Id, purchaseResult);
        return purchaseResult;
    }

    [UserMapping(Default = true)]
    private GameEntityPricing ToGameEntity(ItemPriceBriefDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameItemPurchasePricing>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameEntityPricing ToGameEntity(VehiclePurchasePriceBriefDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameVehiclePurchasePricing>(source.Id, result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameEntityPricing ToGameEntity(VehicleRentalPriceBriefDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntityId<GameVehicleRentalPricing>(source.Id, result);
        return result;
    }

    private void CacheGameEntityId<T>(double? sourceId, IGameEntity result) where T : class, IGameEntity
        => CachedGameEntities[CreateCacheEntityKey<T>(sourceId)] = result;

    private T? ResolveCachedGameEntity<T>(double? sourceId, bool throwOnCacheMiss = true, [CallerArgumentExpression(nameof(sourceId))] string sourceIdExpression = "")
        where T : class, IGameEntity
    {
        var cacheEntityKey = CreateCacheEntityKey<T>(sourceId);
        var cachedEntity = CachedGameEntities.GetValueOrDefault(cacheEntityKey) as T;
        return sourceId > 0 && cachedEntity is null && throwOnCacheMiss
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
    [MapperIgnoreTarget(nameof(GameItemTrait.TraitType))]
    [MapProperty(nameof(ItemAttributeDTO.Id_item), nameof(GameItemTrait.ItemId))]
    [MapProperty(nameof(ItemAttributeDTO.Attribute_name), "fullName")]
    [MapProperty(nameof(ItemAttributeDTO.Value), nameof(GameItemTrait.Content))]
    private partial GameItemTrait MapInternal(ItemAttributeDTO source);

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

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(CommodityPriceBriefDTO.Id_commodity), nameof(GameEntityPricing.OwnerId))]
    [MapProperty(nameof(CommodityPriceBriefDTO.Date_modified), nameof(GameEntityPricing.UpdatedAt), Use = nameof(MapInternalDate))]
    [MapProperty(nameof(CommodityPriceBriefDTO.Price_buy), nameof(GameCommodityPricing.PurchasePrice), Use = nameof(MapInternalMoney))]
    [MapProperty(nameof(CommodityPriceBriefDTO.Price_sell), nameof(GameCommodityPricing.SalePrice), Use = nameof(MapInternalMoney))]
    [MapPropertyFromSource(nameof(GameEntityPricing<IGameEntity>.Terminal), Use = nameof(GetTerminalForCommodityPrice))]
    [MapPropertyFromSource(nameof(GameEntityPricing<IGameEntity>.OwnerId), Use = nameof(GetCommodityForPrice))]
    private partial GameCommodityPricing MapInternalPurchasePrice(CommodityPriceBriefDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(ItemPriceBriefDTO.Id_item), nameof(GameEntityPricing.OwnerId))]
    [MapProperty(nameof(ItemPriceBriefDTO.Date_modified), nameof(GameEntityPricing.UpdatedAt), Use = nameof(MapInternalDate))]
    [MapProperty(nameof(ItemPriceBriefDTO.Price_buy), nameof(GameItemPurchasePricing.PurchasePrice), Use = nameof(MapInternalMoney))]
    [MapProperty(nameof(ItemPriceBriefDTO.Price_sell), nameof(GameItemPurchasePricing.SalePrice), Use = nameof(MapInternalMoney))]
    [MapPropertyFromSource(nameof(GameEntityPricing<IGameEntity>.Terminal), Use = nameof(GetTerminalForItemPrice))]
    [MapPropertyFromSource(nameof(GameEntityPricing<IGameEntity>.OwnerId), Use = nameof(GetItemForPrice))]
    private partial GameItemPurchasePricing MapInternal(ItemPriceBriefDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(VehiclePurchasePriceBriefDTO.Id_vehicle), nameof(GameEntityPricing.OwnerId))]
    [MapProperty(nameof(VehiclePurchasePriceBriefDTO.Date_modified), nameof(GameEntityPricing.UpdatedAt), Use = nameof(MapInternalDate))]
    [MapProperty(nameof(VehiclePurchasePriceBriefDTO.Price_buy), nameof(GameVehiclePurchasePricing.Price), Use = nameof(MapInternalMoney))]
    [MapPropertyFromSource(nameof(GameEntityPricing<IGameEntity>.Terminal), Use = nameof(GetTerminalForVehiclePurchasePrice))]
    [MapPropertyFromSource(nameof(GameEntityPricing<IGameEntity>.OwnerId), Use = nameof(GetVehicleForPurchasePrice))]
    private partial GameVehiclePurchasePricing MapInternal(VehiclePurchasePriceBriefDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapProperty(nameof(VehicleRentalPriceBriefDTO.Id_vehicle), nameof(GameEntityPricing.OwnerId))]
    [MapProperty(nameof(VehicleRentalPriceBriefDTO.Date_modified), nameof(GameEntityPricing.UpdatedAt), Use = nameof(MapInternalDate))]
    [MapProperty(nameof(VehicleRentalPriceBriefDTO.Price_rent), nameof(GameVehicleRentalPricing.Price), Use = nameof(MapInternalMoney))]
    [MapPropertyFromSource(nameof(GameEntityPricing<IGameEntity>.Terminal), Use = nameof(GetTerminalForVehicleRentalPrice))]
    [MapPropertyFromSource(nameof(GameEntityPricing<IGameEntity>.OwnerId), Use = nameof(GetVehicleForRentalPrice))]
    private partial GameVehicleRentalPricing MapInternal(VehicleRentalPriceBriefDTO source);

    [UserMapping(Default = true)]
    private static GameCurrency MapInternalMoney(double? amount)
        => new((int)(amount ?? 0));

    [UserMapping(Default = true)]
    private static DateTimeOffset MapInternalDate(double? timestamp)
        => DateTimeOffset.FromUnixTimeSeconds((long)(timestamp ?? 0));

    [DoesNotReturn]
    private static T ThrowInvalidCacheException<T>(double? sourceId, [CallerArgumentExpression(nameof(sourceId))] string sourceIdExpression = "")
        => throw new ObjectMappingMissingLinkedRelatedObjectException(
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

    private GameTerminal GetTerminalForCommodityPrice(CommodityPriceBriefDTO commodityPrice)
        => ResolveCachedGameEntity<GameTerminal>(commodityPrice.Id_terminal, false)
           ?? FallbackToFakeTerminal(commodityPrice.Id_terminal, commodityPrice.Terminal_name)
           ?? ThrowMissingMappingException<GameTerminal, CommodityPriceBriefDTO>(commodityPrice.Id_terminal);

    private GameTerminal GetTerminalForItemPrice(ItemPriceBriefDTO itemPrice)
        => ResolveCachedGameEntity<GameTerminal>(itemPrice.Id_terminal, false)
           ?? FallbackToFakeTerminal(itemPrice.Id_terminal, itemPrice.Terminal_name)
           ?? ThrowMissingMappingException<GameTerminal, ItemPriceBriefDTO>(itemPrice.Id_terminal);

    private GameTerminal GetTerminalForVehiclePurchasePrice(VehiclePurchasePriceBriefDTO vehiclePurchasePrice)
        => ResolveCachedGameEntity<GameTerminal>(vehiclePurchasePrice.Id_terminal, false)
           ?? FallbackToFakeTerminal(vehiclePurchasePrice.Id_terminal, vehiclePurchasePrice.Terminal_name)
           ?? ThrowMissingMappingException<GameTerminal, VehiclePurchasePriceBriefDTO>(vehiclePurchasePrice.Id_terminal);

    private GameTerminal GetTerminalForVehicleRentalPrice(VehicleRentalPriceBriefDTO vehicleRentalPrice)
        => ResolveCachedGameEntity<GameTerminal>(vehicleRentalPrice.Id_terminal, false)
           ?? FallbackToFakeTerminal(vehicleRentalPrice.Id_terminal, vehicleRentalPrice.Terminal_name)
           ?? ThrowMissingMappingException<GameTerminal, VehicleRentalPriceBriefDTO>(vehicleRentalPrice.Id_terminal);

    private static GameTerminal FallbackToFakeTerminal(double? id, string? name = null, string? code = null)
        => new(
            (int)(id ?? 0),
            name ?? $"Terminal {code ?? id?.ToString("N0", CultureInfo.InvariantCulture) ?? "?"}",
            name ?? $"Terminal {code ?? id?.ToString("N0", CultureInfo.InvariantCulture) ?? "?"}",
            code ?? "",
            GameLocationEntity.Unknown
        )
        {
            Type = GameTerminalType.Undefined,
        };

    private GameCommodity GetCommodityForPrice(CommodityPriceBriefDTO commodityPrice)
        => ResolveCachedGameEntity<GameCommodity>(commodityPrice.Id_commodity)
           ?? ThrowMissingMappingException<GameCommodity, CommodityPriceBriefDTO>(commodityPrice.Id_commodity);

    private GameItem GetItemForPrice(ItemPriceBriefDTO itemPrice)
        => ResolveCachedGameEntity<GameItem>(itemPrice.Id_item)
           ?? ThrowMissingMappingException<GameItem, ItemPriceBriefDTO>(itemPrice.Id_item);

    private GameVehicle GetVehicleForPurchasePrice(VehiclePurchasePriceBriefDTO vehiclePurchasePrice)
        => ResolveCachedGameEntity<GameVehicle>(vehiclePurchasePrice.Id_vehicle)
           ?? ThrowMissingMappingException<GameVehicle, VehiclePurchasePriceBriefDTO>(vehiclePurchasePrice.Id_vehicle);

    private GameVehicle GetVehicleForRentalPrice(VehicleRentalPriceBriefDTO vehicleRentalPrice)
        => ResolveCachedGameEntity<GameVehicle>(vehicleRentalPrice.Id_vehicle)
           ?? ThrowMissingMappingException<GameVehicle, CommodityPriceBriefDTO>(vehicleRentalPrice.Id_vehicle);

    private GameCompany GetCompanyForItem(ItemDTO item)
        => ResolveCachedGameEntity<GameCompany>(item.Id_company)
           ?? GameCompany.Unknown
           ?? ThrowMissingMappingException<GameCompany, ItemDTO>(item.Id);

    private GameCompany GetCompanyForVehicle(VehicleDTO vehicle)
        => ResolveCachedGameEntity<GameCompany>(vehicle.Id_company)
           ?? ThrowMissingMappingException<GameCompany, VehicleDTO>(vehicle.Id);

    private GameProductCategory GetCategoryForItem(ItemDTO item)
        => ResolveCachedGameEntity<GameProductCategory>(item.Id_category)
           ?? ThrowMissingMappingException<GameProductCategory, ItemDTO>(item.Id);
}
