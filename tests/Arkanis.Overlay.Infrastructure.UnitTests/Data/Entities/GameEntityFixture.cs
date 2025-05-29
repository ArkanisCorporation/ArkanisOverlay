namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Entities;

using Domain.Abstractions.Game;
using Domain.Enums;
using Domain.Models.Game;
using Mappers;

internal static class GameEntityFixture
{
    public static GameStarSystem StarSystem { get; } = new(
        (int)(ExternalUexDTOFixture.StarSystem.Id ?? 0),
        ExternalUexDTOFixture.StarSystem.Name!,
        ExternalUexDTOFixture.StarSystem.Code!
    );

    public static GamePlanet Planet { get; } = new(
        (int)(ExternalUexDTOFixture.Planet.Id ?? 0),
        ExternalUexDTOFixture.Planet.Name!,
        ExternalUexDTOFixture.Planet.Code!,
        StarSystem
    );

    public static GameMoon Moon { get; } = new(
        (int)(ExternalUexDTOFixture.Moon.Id ?? 0),
        ExternalUexDTOFixture.Moon.Name!,
        ExternalUexDTOFixture.Moon.Code!,
        Planet
    );

    public static GameCity City { get; } = new(
        (int)(ExternalUexDTOFixture.City.Id ?? 0),
        ExternalUexDTOFixture.City.Name!,
        ExternalUexDTOFixture.City.Code!,
        Planet
    );

    public static GameSpaceStation SpaceStation { get; } = new(
        (int)(ExternalUexDTOFixture.SpaceStation.Id ?? 0),
        ExternalUexDTOFixture.SpaceStation.Name!,
        ExternalUexDTOFixture.SpaceStation.Nickname!,
        City
    );

    public static GameOutpost Outpost { get; } = new(
        (int)(ExternalUexDTOFixture.Outpost.Id ?? 0),
        ExternalUexDTOFixture.Outpost.Name!,
        ExternalUexDTOFixture.Outpost.Nickname!,
        Moon
    );

    public static GameTerminal OutpostCommodityTerminal { get; } = new(
        (int)(ExternalUexDTOFixture.OutpostCommodityTerminal.Id ?? 0),
        ExternalUexDTOFixture.OutpostCommodityTerminal.Name!,
        ExternalUexDTOFixture.OutpostCommodityTerminal.Nickname!,
        ExternalUexDTOFixture.OutpostCommodityTerminal.Code!,
        Outpost
    )
    {
        Type = GameTerminalType.Commodity,
    };

    public static GameCompany ItemCompany { get; } = new(
        (int)(ExternalUexDTOFixture.ItemCompany.Id ?? 0),
        ExternalUexDTOFixture.ItemCompany.Name!,
        ExternalUexDTOFixture.ItemCompany.Nickname!
    );

    public static GameProductCategory ItemLegwearCategory { get; } = new(
        (int)(ExternalUexDTOFixture.ItemLegwearCategory.Id ?? 0),
        ExternalUexDTOFixture.ItemLegwearCategory.Name!,
        ExternalUexDTOFixture.ItemLegwearCategory.Section!
    )
    {
        CategoryType = GameItemCategoryType.Item,
    };

    public static GameProductCategory ItemFootwearCategory { get; } = new(
        (int)(ExternalUexDTOFixture.ItemFootwearCategory.Id ?? 0),
        ExternalUexDTOFixture.ItemFootwearCategory.Name!,
        ExternalUexDTOFixture.ItemFootwearCategory.Section!
    )
    {
        CategoryType = GameItemCategoryType.Item,
    };

    public static GameProductCategory ItemJacketsCategory { get; } = new(
        (int)(ExternalUexDTOFixture.ItemJacketsCategory.Id ?? 0),
        ExternalUexDTOFixture.ItemJacketsCategory.Name!,
        ExternalUexDTOFixture.ItemJacketsCategory.Section!
    )
    {
        CategoryType = GameItemCategoryType.Item,
    };

    public static GameItem Item1 { get; } = new(
        (int)(ExternalUexDTOFixture.Item1.Id ?? 0),
        ExternalUexDTOFixture.Item1.Uuid!,
        ItemCompany,
        ItemLegwearCategory
    );

    public static GameItem Item2 { get; } = new(
        (int)(ExternalUexDTOFixture.Item2.Id ?? 0),
        ExternalUexDTOFixture.Item2.Uuid!,
        ItemCompany,
        ItemFootwearCategory
    );

    public static GameItem Item3 { get; } = new(
        (int)(ExternalUexDTOFixture.Item3.Id ?? 0),
        ExternalUexDTOFixture.Item3.Uuid!,
        ItemCompany,
        ItemJacketsCategory
    );

    public static IGameEntity[] AllEntities
        =>
        [
            StarSystem,
            Planet,
            Moon,
            SpaceStation,
            City,
            Outpost,
            OutpostCommodityTerminal,
            ItemCompany,
            ItemLegwearCategory,
            ItemFootwearCategory,
            ItemJacketsCategory,
            Item1,
            Item2,
            Item3,
        ];
}
