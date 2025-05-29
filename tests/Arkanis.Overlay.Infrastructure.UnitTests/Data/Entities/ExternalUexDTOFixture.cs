namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Entities;

using Domain.Models.Inventory;
using Infrastructure.Data.Entities;

internal static class DatabaseInventoryEntitiesFixture
{
    public static readonly PhysicalItemInventoryEntryEntity PhysicalItem1 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        ItemId = GameEntityFixture.Item1.StrongId,
        Quantity = new Quantity(1, Quantity.Type.StandardCargoUnit),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly PhysicalItemInventoryEntryEntity PhysicalItem2 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        ItemId = GameEntityFixture.Item2.StrongId,
        Quantity = new Quantity(2, Quantity.Type.CentiStandardCargoUnit),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly PhysicalItemInventoryEntryEntity PhysicalItem3 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        ItemId = GameEntityFixture.Item3.StrongId,
        Quantity = new Quantity(10, Quantity.Type.Count),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly InventoryEntryListEntity ItemList = new()
    {
        Id = InventoryEntryListId.CreateNew(),
        Name = nameof(ItemList),
        Notes = "This is for a unit test",
        Entities = [PhysicalItem1, PhysicalItem2],
    };

    public static readonly PhysicalCommodityInventoryEntryEntity PhysicalCommodity1 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        CommodityId = GameEntityFixture.Commodity1.StrongId,
        Quantity = new Quantity(5, Quantity.Type.StandardCargoUnit),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly PhysicalCommodityInventoryEntryEntity PhysicalCommodity2 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        CommodityId = GameEntityFixture.Commodity2.StrongId,
        Quantity = new Quantity(8, Quantity.Type.StandardCargoUnit),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly PhysicalCommodityInventoryEntryEntity PhysicalCommodity3 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        CommodityId = GameEntityFixture.Commodity3.StrongId,
        Quantity = new Quantity(300, Quantity.Type.CentiStandardCargoUnit),
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly InventoryEntryListEntity CommodityList = new()
    {
        Id = InventoryEntryListId.CreateNew(),
        Name = nameof(CommodityList),
        Notes = "This is for a unit test",
        Entities = [PhysicalCommodity1, PhysicalCommodity2],
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
        => ItemList.Entities.Sort(Comparison);

    public static int Comparison(InventoryEntryEntityBase left, InventoryEntryEntityBase right)
        => left.Id.Identity.CompareTo(right.Id.Identity);
}
