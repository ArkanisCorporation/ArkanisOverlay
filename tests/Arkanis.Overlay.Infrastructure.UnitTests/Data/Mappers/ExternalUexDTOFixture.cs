namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Mappers;

using External.UEX.Abstractions;

public static class ExternalUexDTOFixture
{
    public static CompanyDTO ItemCompany { get; } = new()
    {
        Id = 55,
        Is_item_manufacturer = 1,
        Is_vehicle_manufacturer = 0,
        Name = "Code Blue Apparel",
        Nickname = "Code Blue",
    };

    public static CategoryDTO ItemLegwearCategory { get; } = new()
    {
        Id = 13,
        Type = "item",
        Section = "Clothing",
        Name = "Legwear",
        Is_game_related = 1,
        Is_mining = 0,
    };

    public static CategoryDTO ItemFootwearCategory { get; } = new()
    {
        Id = 8,
        Type = "item",
        Section = "Clothing",
        Name = "Footwear",
        Is_game_related = 1,
        Is_mining = 0,
    };

    public static CategoryDTO ItemJacketsCategory { get; } = new()
    {
        Id = 11,
        Type = "item",
        Section = "Clothing",
        Name = "Jackets",
        Is_game_related = 1,
        Is_mining = 0,
    };

    public static ItemDTO Item1 { get; } = new()
    {
        Id = 3332,
        Uuid = "26929685-659d-460c-b6c9-d3286ff9ab44",
        Id_category = ItemLegwearCategory.Id,
        Id_company = ItemCompany.Id,
        Name = "Ajax Security Uniform Pants",
        Section = "Clothing",
        Category = "Legwear",
    };

    public static ItemDTO Item2 { get; } = new()
    {
        Id = 3333,
        Uuid = "bdfed6de-7e10-4b47-a229-5493ab7576a6",
        Id_category = ItemFootwearCategory.Id,
        Id_company = ItemCompany.Id,
        Name = "Ajax Security Uniform Shoes",
        Section = "Clothing",
        Category = "Footwear",
    };

    public static ItemDTO Item3 { get; } = new()
    {
        Id = 3334,
        Uuid = "0aba253a-f590-4ea0-88ae-fa34a5c2d030",
        Id_category = ItemJacketsCategory.Id,
        Id_company = ItemCompany.Id,
        Name = "Ajax Security Uniform Jacket",
        Section = "Clothing",
        Category = "Jackets",
    };

    public static UniverseStarSystemDTO StarSystem { get; } = new()
    {
        Id = 68,
        Name = "Stanton",
        Code = "ST",
    };

    public static UniversePlanetDTO Planet { get; } = new()
    {
        Id = 59,
        Id_star_system = StarSystem.Id,
        Name = "Crusader",
        Code = "CRU",
    };

    public static UniverseMoonDTO Moon { get; } = new()
    {
        Id = 25,
        Id_star_system = StarSystem.Id,
        Id_planet = Planet.Id,
        Name = "Daymar",
        Code = "DAY",
    };

    public static UniverseCityDTO City { get; } = new()
    {
        Id = 5,
        Id_star_system = StarSystem.Id,
        Id_planet = Planet.Id,
        Name = "Orison",
        Code = "ORIS",
    };

    public static UniverseSpaceStationDTO SpaceStation { get; } = new()
    {
        Id = 27,
        Id_star_system = StarSystem.Id,
        Id_planet = Planet.Id,
        Id_city = City.Id,
        Name = "Seraphim Station",
        Nickname = "Seraphim",
    };

    public static UniverseOutpostDTO Outpost { get; } = new()
    {
        Id = 48,
        Id_star_system = StarSystem.Id,
        Id_moon = Moon.Id,
        Name = "Shubin Mining Facility SCD-1",
        Nickname = "Shubin SCD-1",
    };

    public static UniverseTerminalDTO OutpostCommodityTerminal { get; } = new()
    {
        Id = 79,
        Id_star_system = StarSystem.Id,
        Id_moon = Moon.Id,
        Id_outpost = Outpost.Id,
        Name = "Shubin Mining Facility SCD-1",
        Nickname = "Shubin SCD-1",
        Code = "SSCD1",
        Type = "commodity",
    };

    public static CommodityDTO Commodity1 { get; } = new()
    {
        Id = 1,
        Id_parent = 2,
        Name = "Agricium",
        Code = "AGRI",
        Kind = "Metal",
    };

    public static CommodityDTO Commodity2 { get; } = new()
    {
        Id = 3,
        Id_parent = 0,
        Name = "Agricultural Supplies",
        Code = "AGRS",
        Kind = "Agricultural",
    };

    public static CommodityDTO Commodity3 { get; } = new()
    {
        Id = 58,
        Id_parent = 59,
        Name = "Quantainium",
        Code = "QUAN",
        Kind = "Mineral",
    };
}
