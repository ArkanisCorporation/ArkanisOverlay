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
    private readonly ConcurrentDictionary<UexApiGameEntityId, IGameEntity> _cachedGameEntities = [];

    public async ValueTask<IGameEntity> ToGameEntityAsync<TSource>(TSource source) where TSource : class
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
            CommodityRouteDTO tradeRoute => ToGameEntity(tradeRoute),
            ItemPriceBriefDTO itemPrice => ToGameEntity(itemPrice),
            CommodityPriceBriefDTO commodityPrice => ToGameEntity(commodityPrice),
            VehiclePurchasePriceBriefDTO vehiclePurchasePrice => ToGameEntity(vehiclePurchasePrice),
            VehicleRentalPriceBriefDTO vehicleRentalPrice => ToGameEntity(vehicleRentalPrice),
            MarketplaceListingDTO marketplaceListing => ToGameEntity(marketplaceListing),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Cannot map to game entity from unsupported source type."),
        };

        await hydrationService.HydrateAsync(result);
        return result;
    }

    public UexApiGameEntityId CreateGameEntityId<TSource>(TSource source, Func<TSource, double?> getSourceId) where TSource : class
        => source switch
        {
            UniverseStarSystemDTO => UexApiGameEntityId.Create<GameStarSystem>(getSourceId(source) ?? 0),
            UniversePlanetDTO => UexApiGameEntityId.Create<GamePlanet>(getSourceId(source) ?? 0),
            UniverseMoonDTO => UexApiGameEntityId.Create<GameMoon>(getSourceId(source) ?? 0),
            UniverseSpaceStationDTO => UexApiGameEntityId.Create<GameSpaceStation>(getSourceId(source) ?? 0),
            UniverseCityDTO => UexApiGameEntityId.Create<GameCity>(getSourceId(source) ?? 0),
            UniverseOutpostDTO => UexApiGameEntityId.Create<GameOutpost>(getSourceId(source) ?? 0),
            UniverseTerminalDTO => UexApiGameEntityId.Create<GameTerminal>(getSourceId(source) ?? 0),
            CommodityDTO => UexApiGameEntityId.Create<GameCommodity>(getSourceId(source) ?? 0),
            ItemDTO => UexApiGameEntityId.Create<GameItem>(getSourceId(source) ?? 0),
            ItemAttributeDTO => UexApiGameEntityId.Create<GameItemTrait>(getSourceId(source) ?? 0),
            CompanyDTO => UexApiGameEntityId.Create<GameCompany>(getSourceId(source) ?? 0),
            CategoryDTO => UexApiGameEntityId.Create<GameProductCategory>(getSourceId(source) ?? 0),
            VehicleDTO => UexApiGameEntityId.Create<GameVehicle>(getSourceId(source) ?? 0),
            CommodityRouteDTO => UexApiGameEntityId.Create<GameTradeRoute>(getSourceId(source) ?? 0),
            ItemPriceBriefDTO => UexApiGameEntityId.Create<IPriceOf<GameItem, GameEntityTradePrice>>(getSourceId(source) ?? 0),
            CommodityPriceBriefDTO => UexApiGameEntityId.Create<IPriceOf<GameCommodity, GameEntityTradePrice>>(getSourceId(source) ?? 0),
            VehiclePurchasePriceBriefDTO => UexApiGameEntityId.Create<IPriceOf<GameVehicle, GameEntityPurchasePrice>>(getSourceId(source) ?? 0),
            VehicleRentalPriceBriefDTO => UexApiGameEntityId.Create<IPriceOf<GameVehicle, GameEntityRentalPrice>>(getSourceId(source) ?? 0),
            MarketplaceListingDTO => UexApiGameEntityId.Create<IPriceOf<GameEntity, GameEntityMarketPrice>>(getSourceId(source) ?? 0),
            _ => throw new InvalidOperationException(
                $"Unable to create internal {typeof(UexApiGameEntityId)}, unsupported source game entity type: {source.GetType()}"
            ),
        };

    [UserMapping(Default = true)]
    private GameStarSystem ToGameEntity(UniverseStarSystemDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GamePlanet ToGameEntity(UniversePlanetDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameMoon ToGameEntity(UniverseMoonDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameCity ToGameEntity(UniverseCityDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameSpaceStation ToGameEntity(UniverseSpaceStationDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameOutpost ToGameEntity(UniverseOutpostDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameTerminal ToGameEntity(UniverseTerminalDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameCommodity ToGameEntity(CommodityDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameItem ToGameEntity(ItemDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameItemTrait ToGameEntity(ItemAttributeDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameCompany ToGameEntity(CompanyDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameProductCategory ToGameEntity(CategoryDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
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
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameTradeRoute ToGameEntity(CommodityRouteDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameEntityPrice ToGameEntity(CommodityPriceBriefDTO source)
    {
        var purchaseResult = MapInternal(source);
        CacheGameEntity(purchaseResult);
        return purchaseResult;
    }

    [UserMapping(Default = true)]
    private GameEntityPrice ToGameEntity(ItemPriceBriefDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameEntityPrice ToGameEntity(VehiclePurchasePriceBriefDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameEntityPrice ToGameEntity(VehicleRentalPriceBriefDTO source)
    {
        var result = MapInternal(source);
        CacheGameEntity(result);
        return result;
    }

    [UserMapping(Default = true)]
    private GameEntityPrice ToGameEntity(MarketplaceListingDTO source)
    {
        GameEntityMarketPrice result = source.Operation switch
        {
            "buy" => MapInternalSalePrice(source), // someone wants to buy on the marketplace, so the price is the sale price
            "sell" => MapInternalPurchasePrice(source), // someone wants to sell on the marketplace, so the price is the purchase price
            _ => throw new ArgumentOutOfRangeException(nameof(source), source.Operation, "Unable to select marketplace price type based on operation."),
        };
        CacheGameEntity(result);
        return result;
    }

    internal void CacheGameEntity(IGameEntity result)
        => _cachedGameEntities[result.Id] = result;

    public T ResolveCachedGameEntity<T>(UexId<T> id)
        where T : class, IGameEntity
        => _cachedGameEntities.GetValueOrDefault(id) as T ?? ThrowInvalidCacheException<T>(id.Identity);

    public T ResolveCachedGameEntity<T>(UexApiGameEntityId id)
        where T : class, IGameEntity
        => _cachedGameEntities.GetValueOrDefault(id) as T ?? ThrowInvalidCacheException<T>(id.Identity);

    private T? ResolveCachedGameEntity<T>(double? sourceId, bool throwOnCacheMiss = true, [CallerArgumentExpression("sourceId")] string sourceIdExpression = "")
        where T : class, IGameEntity
    {
        var entityId = UexApiGameEntityId.Create<T>(sourceId ?? 0);
        var cachedEntity = _cachedGameEntities.GetValueOrDefault(entityId) as T;
        return sourceId > 0 && cachedEntity is null && throwOnCacheMiss
            ? ThrowInvalidCacheException<T>(sourceId, sourceIdExpression)
            : cachedEntity;
    }

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
    [MapProperty(nameof(UniverseTerminalDTO.Is_available_live), nameof(GameTerminal.IsAvailable))]
    [MapProperty(nameof(UniverseTerminalDTO.Is_auto_load), nameof(GameTerminal.IsAutoLoad))]
    [MapProperty(nameof(UniverseTerminalDTO.Max_container_size), nameof(GameTerminal.MaxContainerSize))]
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
    [MapProperty(nameof(CommodityRouteDTO.Price_margin), nameof(GameTradeRoute.PriceMarginPercent))]
    [MapProperty(nameof(CommodityRouteDTO.Price_roi), nameof(GameTradeRoute.PriceReturnOnInvestmentPercent))]
    [MapPropertyFromSource(nameof(GameTradeRoute.Commodity), Use = nameof(GetCommodityForTradeRoute))]
    [MapPropertyFromSource(nameof(GameTradeRoute.Origin), Use = nameof(MapTradeRouteOriginParty))]
    [MapPropertyFromSource(nameof(GameTradeRoute.Destination), Use = nameof(MapTradeRouteDestinationParty))]
    private partial GameTradeRoute MapInternal(CommodityRouteDTO source);

    [MapPropertyFromSource(nameof(GameTradeRoute.Party.Terminal), Use = nameof(GetOriginTerminalForTradeRoute))]
    [MapProperty(nameof(CommodityRouteDTO.Price_origin), nameof(GameTradeRoute.Party.Price))]
    [MapProperty(nameof(CommodityRouteDTO.Scu_origin), nameof(GameTradeRoute.Party.CargoUnitsAvailable))]
    [MapProperty(nameof(CommodityRouteDTO.Status_origin), nameof(GameTradeRoute.Party.InventoryStatus))]
    private partial GameTradeRoute.Party MapTradeRouteOriginParty(CommodityRouteDTO source);

    [MapPropertyFromSource(nameof(GameTradeRoute.Party.Terminal), Use = nameof(GetDestinationTerminalForTradeRoute))]
    [MapProperty(nameof(CommodityRouteDTO.Price_destination), nameof(GameTradeRoute.Party.Price))]
    [MapProperty(nameof(CommodityRouteDTO.Scu_destination), nameof(GameTradeRoute.Party.CargoUnitsAvailable))]
    [MapProperty(nameof(CommodityRouteDTO.Status_destination), nameof(GameTradeRoute.Party.InventoryStatus))]
    private partial GameTradeRoute.Party MapTradeRouteDestinationParty(CommodityRouteDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapPropertyFromSource(nameof(GameEntityPrice.Id), Use = nameof(GetEntityIdForCommodityPrice))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetReferencedEntityIdForCommodityPrice))]
    [MapProperty(nameof(CommodityPriceBriefDTO.Date_modified), nameof(GameEntityPrice.UpdatedAt), Use = nameof(MapInternalDate))]
    [MapProperty(nameof(CommodityPriceBriefDTO.Price_buy), nameof(GameEntityTradePrice.PurchasePrice), Use = nameof(MapInternalMoney))]
    [MapProperty(nameof(CommodityPriceBriefDTO.Price_sell), nameof(GameEntityTradePrice.SalePrice), Use = nameof(MapInternalMoney))]
    [MapPropertyFromSource(nameof(GameEntityTerminalPrice.Terminal), Use = nameof(GetTerminalForCommodityPrice))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetCommodityForPrice))]
    private partial GameEntityTradePrice MapInternal(CommodityPriceBriefDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapPropertyFromSource(nameof(GameEntityPrice.Id), Use = nameof(GetEntityIdForItemPrice))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetReferencedEntityIdForItemPrice))]
    [MapProperty(nameof(ItemPriceBriefDTO.Date_modified), nameof(GameEntityPrice.UpdatedAt), Use = nameof(MapInternalDate))]
    [MapProperty(nameof(ItemPriceBriefDTO.Price_buy), nameof(GameEntityTradePrice.PurchasePrice), Use = nameof(MapInternalMoney))]
    [MapProperty(nameof(ItemPriceBriefDTO.Price_sell), nameof(GameEntityTradePrice.SalePrice), Use = nameof(MapInternalMoney))]
    [MapPropertyFromSource(nameof(GameEntityTerminalPrice.Terminal), Use = nameof(GetTerminalForItemPrice))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetItemForPrice))]
    private partial GameEntityTradePrice MapInternal(ItemPriceBriefDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapPropertyFromSource(nameof(GameEntityPrice.Id), Use = nameof(GetEntityIdForVehiclePurchasePrice))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetReferencedEntityIdForVehiclePurchasePrice))]
    [MapProperty(nameof(VehiclePurchasePriceBriefDTO.Date_modified), nameof(GameEntityPrice.UpdatedAt), Use = nameof(MapInternalDate))]
    [MapProperty(nameof(VehiclePurchasePriceBriefDTO.Price_buy), nameof(GameEntityPurchasePrice.PurchasePrice), Use = nameof(MapInternalMoney))]
    [MapPropertyFromSource(nameof(GameEntityTerminalPrice.Terminal), Use = nameof(GetTerminalForVehiclePurchasePrice))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetVehicleForPurchasePrice))]
    private partial GameEntityPurchasePrice MapInternal(VehiclePurchasePriceBriefDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapPropertyFromSource(nameof(GameEntityPrice.Id), Use = nameof(GetEntityIdForVehicleRentalPrice))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetReferencedEntityIdForVehicleRentalPrice))]
    [MapProperty(nameof(VehicleRentalPriceBriefDTO.Date_modified), nameof(GameEntityPrice.UpdatedAt), Use = nameof(MapInternalDate))]
    [MapProperty(nameof(VehicleRentalPriceBriefDTO.Price_rent), nameof(GameEntityRentalPrice.RentalPrice), Use = nameof(MapInternalMoney))]
    [MapPropertyFromSource(nameof(GameEntityTerminalPrice.Terminal), Use = nameof(GetTerminalForVehicleRentalPrice))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetVehicleForRentalPrice))]
    private partial GameEntityRentalPrice MapInternal(VehicleRentalPriceBriefDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapValue(nameof(GameEntityMarketPrice.MarketName), "UEX")]
    [MapPropertyFromSource(nameof(GameEntityPrice.Id), Use = nameof(GetEntityIdForMarketplaceListing))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetReferencedEntityIdForMarketplaceListing))]
    [MapProperty(nameof(MarketplaceListingDTO.Date_added), nameof(GameEntityPrice.UpdatedAt), Use = nameof(MapInternalDate))]
    [MapProperty(nameof(MarketplaceListingDTO.Price), nameof(GameEntityMarketPurchasePrice.PurchasePrice), Use = nameof(MapInternalMoney))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetEntityForMarketPrice))]
    private partial GameEntityMarketPurchasePrice MapInternalPurchasePrice(MarketplaceListingDTO source);

    [MapperIgnoreTarget(nameof(GameEntity.Name))]
    [MapValue(nameof(GameEntityMarketPrice.MarketName), "UEX")]
    [MapPropertyFromSource(nameof(GameEntityPrice.Id), Use = nameof(GetEntityIdForMarketplaceListing))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetReferencedEntityIdForMarketplaceListing))]
    [MapProperty(nameof(MarketplaceListingDTO.Date_added), nameof(GameEntityPrice.UpdatedAt), Use = nameof(MapInternalDate))]
    [MapProperty(nameof(MarketplaceListingDTO.Price), nameof(GameEntityMarketSalePrice.SalePrice), Use = nameof(MapInternalMoney))]
    [MapPropertyFromSource(nameof(GameEntityPrice.OwnerId), Use = nameof(GetEntityForMarketPrice))]
    private partial GameEntityMarketSalePrice MapInternalSalePrice(MarketplaceListingDTO source);

    [UserMapping(Default = true)]
    private static GameCurrency MapInternalMoney(double? amount)
        => new((int)(amount ?? 0));

    [UserMapping(Default = true)]
    private static bool MapInternalBoolean(double? boolean)
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        => boolean == 1;

    [UserMapping(Default = true)]
    private static DateTimeOffset MapInternalDate(double? timestamp)
        => DateTimeOffset.FromUnixTimeSeconds((long)(timestamp ?? 0));

    [DoesNotReturn]
    private static T ThrowInvalidCacheException<T>(double? sourceId, [CallerArgumentExpression("sourceId")] string sourceIdExpression = "")
        => throw new ObjectMappingMissingLinkedRelatedObjectException(
            $"Could not resolve cached entity instance of {typeof(T)} for: {sourceIdExpression} == {sourceId}"
        );

    [DoesNotReturn]
    private static TTarget ThrowMissingMappingException<TTarget, TSource>(double? sourceId)
        => throw new ObjectMappingException($"Could not find correct mapping for {typeof(TTarget)} in {typeof(TSource)} with ID {sourceId}.", null);

    private UexApiGameEntityId GetEntityIdForVehiclePurchasePrice(VehiclePurchasePriceBriefDTO source)
        => CreateGameEntityId(source, dto => dto.Id);

    private UexApiGameEntityId GetEntityIdForVehicleRentalPrice(VehicleRentalPriceBriefDTO source)
        => CreateGameEntityId(source, dto => dto.Id);

    private UexApiGameEntityId GetEntityIdForCommodityPrice(CommodityPriceBriefDTO source)
        => CreateGameEntityId(source, dto => dto.Id);

    private UexApiGameEntityId GetEntityIdForMarketplaceListing(MarketplaceListingDTO source)
        => CreateGameEntityId(source, dto => dto.Id);

    private UexApiGameEntityId GetEntityIdForItemPrice(ItemPriceBriefDTO source)
        => CreateGameEntityId(source, dto => dto.Id);

    private UexApiGameEntityId GetReferencedEntityIdForVehiclePurchasePrice(VehiclePurchasePriceBriefDTO source)
        => UexApiGameEntityId.Create<GameVehicle>(source.Id_vehicle ?? 0);

    private UexApiGameEntityId GetReferencedEntityIdForVehicleRentalPrice(VehicleRentalPriceBriefDTO source)
        => UexApiGameEntityId.Create<GameVehicle>(source.Id_vehicle ?? 0);

    private UexApiGameEntityId GetReferencedEntityIdForCommodityPrice(CommodityPriceBriefDTO source)
        => UexApiGameEntityId.Create<GameCommodity>(source.Id_commodity ?? 0);

    private UexApiGameEntityId GetReferencedEntityIdForMarketplaceListing(MarketplaceListingDTO source)
        => UexApiGameEntityId.Create<GameItem>(source.Id_item ?? 0);

    private UexApiGameEntityId GetReferencedEntityIdForItemPrice(ItemPriceBriefDTO source)
        => UexApiGameEntityId.Create<GameItem>(source.Id_item ?? 0);

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

    private GameTerminal GetOriginTerminalForTradeRoute(CommodityRouteDTO commodityRoute)
        => ResolveCachedGameEntity<GameTerminal>(commodityRoute.Id_terminal_origin)
           ?? ThrowMissingMappingException<GameTerminal, CommodityRouteDTO>(commodityRoute.Id_terminal_origin);

    private GameTerminal GetDestinationTerminalForTradeRoute(CommodityRouteDTO commodityRoute)
        => ResolveCachedGameEntity<GameTerminal>(commodityRoute.Id_terminal_destination)
           ?? ThrowMissingMappingException<GameTerminal, CommodityRouteDTO>(commodityRoute.Id_terminal_destination);

    private GameCommodity GetCommodityForTradeRoute(CommodityRouteDTO commodityRoute)
        => ResolveCachedGameEntity<GameCommodity>(commodityRoute.Id_commodity)
           ?? ThrowMissingMappingException<GameCommodity, CommodityRouteDTO>(commodityRoute.Id_commodity);

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

    private GameEntity GetEntityForMarketPrice(MarketplaceListingDTO item)
        => ThrowMissingMappingException<GameEntity, MarketplaceListingDTO>(item.Id);

    private interface IPriceOf<TEntity, TPrice> : IGameEntity
        where TEntity : IGameEntity
        where TPrice : class, IGameEntityPrice;
}
