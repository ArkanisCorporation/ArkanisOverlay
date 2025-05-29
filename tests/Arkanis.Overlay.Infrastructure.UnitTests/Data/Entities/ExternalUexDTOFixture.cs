namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Entities;

using Domain.Models.Inventory;
using Infrastructure.Data.Entities;

internal static class DatabaseInventoryEntitiesFixture
{
    public static readonly PhysicalItemInventoryEntryEntity PhysicalItem1 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        ItemId = GameEntityFixture.Item1.StrongId,
        Quantity = new Quantity(1, Quantity.UnitType.StandardCargoUnit),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly PhysicalItemInventoryEntryEntity PhysicalItem2 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        ItemId = GameEntityFixture.Item2.StrongId,
        Quantity = new Quantity(2, Quantity.UnitType.CentiStandardCargoUnit),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly PhysicalItemInventoryEntryEntity PhysicalItem3 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        ItemId = GameEntityFixture.Item3.StrongId,
        Quantity = new Quantity(10, Quantity.UnitType.Count),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly InventoryEntryListEntity ItemList = new()
    {
        Id = InventoryEntryListId.CreateNew(),
        Name = nameof(ItemList),
        Notes = "This is for a unit test",
        Entries = [PhysicalItem1, PhysicalItem2],
    };

    public static readonly PhysicalCommodityInventoryEntryEntity PhysicalCommodity1 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        CommodityId = GameEntityFixture.Commodity1.StrongId,
        Quantity = new Quantity(5, Quantity.UnitType.StandardCargoUnit),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly PhysicalCommodityInventoryEntryEntity PhysicalCommodity2 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        CommodityId = GameEntityFixture.Commodity2.StrongId,
        Quantity = new Quantity(8, Quantity.UnitType.StandardCargoUnit),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly PhysicalCommodityInventoryEntryEntity PhysicalCommodity3 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        CommodityId = GameEntityFixture.Commodity3.StrongId,
        Quantity = new Quantity(300, Quantity.UnitType.CentiStandardCargoUnit),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly InventoryEntryListEntity CommodityList = new()
    {
        Id = InventoryEntryListId.CreateNew(),
        Name = nameof(CommodityList),
        Notes = "This is for a unit test",
        Entries = [PhysicalCommodity1, PhysicalCommodity2],
    };

    public static readonly IEnumerable<InventoryEntryEntityBase> AllEntries =
    [
        PhysicalItem1,
        PhysicalItem2,
        PhysicalItem3,
        PhysicalCommodity1,
        PhysicalCommodity2,
        PhysicalCommodity3,
    ];

    public static readonly IEnumerable<InventoryEntryListEntity> AllLists = [ItemList, CommodityList];

    static DatabaseInventoryEntitiesFixture()
        => ItemList.Entries.Sort(Comparison);

    public static int Comparison(InventoryEntryEntityBase left, InventoryEntryEntityBase right)
        => left.Id.Identity.CompareTo(right.Id.Identity);
}
