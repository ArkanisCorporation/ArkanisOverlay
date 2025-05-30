namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Entities;

using Domain.Models.Inventory;

public static class DomainInventoryEntriesFixture
{
    public static readonly PhysicalItemInventoryEntry PhysicalItem1 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Item = GameEntityFixture.Item1,
        Quantity = new Quantity(1, Quantity.Type.Count),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly PhysicalItemInventoryEntry PhysicalItem2 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Item = GameEntityFixture.Item2,
        Quantity = new Quantity(2, Quantity.Type.Count),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly PhysicalItemInventoryEntry PhysicalItem3 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Item = GameEntityFixture.Item3,
        Quantity = new Quantity(10, Quantity.Type.Count),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly InventoryEntryList ItemList = new()
    {
        Id = InventoryEntryListId.CreateNew(),
        Name = nameof(ItemList),
        Notes = "This is for a unit test",
        Entries = [PhysicalItem1, PhysicalItem2],
    };

    public static readonly PhysicalCommodityInventoryEntry PhysicalCommodity1 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Commodity = GameEntityFixture.Commodity1,
        Quantity = new Quantity(5, Quantity.Type.StandardCargoUnit),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly PhysicalCommodityInventoryEntry PhysicalCommodity2 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Commodity = GameEntityFixture.Commodity2,
        Quantity = new Quantity(8, Quantity.Type.StandardCargoUnit),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly PhysicalCommodityInventoryEntry PhysicalCommodity3 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Commodity = GameEntityFixture.Commodity3,
        Quantity = new Quantity(300, Quantity.Type.CentiStandardCargoUnit),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly InventoryEntryList CommodityList = new()
    {
        Id = InventoryEntryListId.CreateNew(),
        Name = nameof(CommodityList),
        Notes = "This is for a unit test",
        Entries = [PhysicalCommodity1, PhysicalCommodity2],
    };

    public static readonly IEnumerable<InventoryEntryBase> AllEntries =
    [
        PhysicalItem1,
        PhysicalItem2,
        PhysicalItem3,
        PhysicalCommodity1,
        PhysicalCommodity2,
        PhysicalCommodity3,
    ];

    public static readonly IEnumerable<InventoryEntryList> AllLists = [ItemList, CommodityList];

    static DomainInventoryEntriesFixture()
        => ItemList.Entries.Sort(Comparison);

    public static int Comparison(InventoryEntryBase left, InventoryEntryBase right)
        => left.Id.Identity.CompareTo(right.Id.Identity);
}
