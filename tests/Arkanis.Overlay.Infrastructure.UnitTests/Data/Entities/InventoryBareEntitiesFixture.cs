namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Entities;

using Domain.Models.Inventory;
using Domain.Models.Trade;

public static class DomainInventoryEntriesFixture
{
    public static readonly LocationInventoryEntry LocationItem1 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = QuantityOf.Create(GameEntityFixture.Item1, new Quantity(1, Quantity.UnitType.Count)),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly LocationInventoryEntry LocationItem2 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = QuantityOf.Create(GameEntityFixture.Item2, new Quantity(2, Quantity.UnitType.Count)),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly LocationInventoryEntry LocationItem3 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = QuantityOf.Create(GameEntityFixture.Item3, new Quantity(10, Quantity.UnitType.Count)),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly InventoryEntryList ItemList = new()
    {
        Id = InventoryEntryListId.CreateNew(),
        Name = nameof(ItemList),
        Notes = "This is for a unit test",
        Entries = [LocationItem1, LocationItem2],
    };

    public static readonly LocationInventoryEntry LocationCommodity1 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = QuantityOf.Create(GameEntityFixture.Commodity1, new Quantity(5, Quantity.UnitType.StandardCargoUnit)),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly LocationInventoryEntry LocationCommodity2 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = QuantityOf.Create(GameEntityFixture.Commodity2, new Quantity(8, Quantity.UnitType.StandardCargoUnit)),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly LocationInventoryEntry LocationCommodity3 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = QuantityOf.Create(GameEntityFixture.Commodity3, new Quantity(300, Quantity.UnitType.CentiStandardCargoUnit)),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly InventoryEntryList CommodityList = new()
    {
        Id = InventoryEntryListId.CreateNew(),
        Name = nameof(CommodityList),
        Notes = "This is for a unit test",
        Entries = [LocationCommodity1, LocationCommodity2],
    };

    public static readonly IEnumerable<InventoryEntryBase> AllEntries =
    [
        LocationItem1,
        LocationItem2,
        LocationItem3,
        LocationCommodity1,
        LocationCommodity2,
        LocationCommodity3,
    ];

    public static readonly IEnumerable<InventoryEntryList> AllLists = [ItemList, CommodityList];

    static DomainInventoryEntriesFixture()
        => ItemList.Entries.Sort(Comparison);

    public static int Comparison(InventoryEntryBase left, InventoryEntryBase right)
        => left.Id.Identity.CompareTo(right.Id.Identity);
}
