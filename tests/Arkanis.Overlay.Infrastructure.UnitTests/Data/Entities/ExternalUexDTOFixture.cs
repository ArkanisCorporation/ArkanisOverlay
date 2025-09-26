namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Entities;

using Domain.Models.Inventory;
using Infrastructure.Data.Entities;

internal static class DatabaseInventoryEntitiesFixture
{
    public static readonly LocationInventoryEntryEntity LocationItem1 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = new QuantityOfEntity
        {
            Amount = 1,
            Unit = Quantity.UnitType.StandardCargoUnit,
            Reference = new OwnableEntityReferenceEntity
            {
                EntityId = GameEntityFixture.Item1.Id,
            },
        },
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly LocationInventoryEntryEntity LocationItem2 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = new QuantityOfEntity
        {
            Amount = 100,
            Unit = Quantity.UnitType.CentiStandardCargoUnit,
            Reference = new OwnableEntityReferenceEntity
            {
                EntityId = GameEntityFixture.Item2.Id,
            },
        },
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly LocationInventoryEntryEntity LocationItem3 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = new QuantityOfEntity
        {
            Amount = 10,
            Unit = Quantity.UnitType.Count,
            Reference = new OwnableEntityReferenceEntity
            {
                EntityId = GameEntityFixture.Item3.Id,
            },
        },
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly InventoryEntryListEntity ItemList = new()
    {
        Id = InventoryEntryListId.CreateNew(),
        Name = nameof(ItemList),
        Notes = "This is for a unit test",
        Entries = [LocationItem1, LocationItem2],
    };

    public static readonly LocationInventoryEntryEntity PhysicalCommodity1 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = new QuantityOfEntity
        {
            Amount = 5,
            Unit = Quantity.UnitType.StandardCargoUnit,
            Reference = new OwnableEntityReferenceEntity
            {
                EntityId = GameEntityFixture.Commodity1.Id,
            },
        },
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly LocationInventoryEntryEntity PhysicalCommodity2 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = new QuantityOfEntity
        {
            Amount = 24,
            Unit = Quantity.UnitType.StandardCargoUnit,
            Reference = new OwnableEntityReferenceEntity
            {
                EntityId = GameEntityFixture.Commodity2.Id,
            },
        },
        LocationId = GameEntityFixture.SpaceStation.Id,
    };

    public static readonly LocationInventoryEntryEntity PhysicalCommodity3 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Quantity = new QuantityOfEntity
        {
            Amount = 300,
            Unit = Quantity.UnitType.StandardCargoUnit,
            Reference = new OwnableEntityReferenceEntity
            {
                EntityId = GameEntityFixture.Commodity3.Id,
            },
        },
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
        LocationItem1,
        LocationItem2,
        LocationItem3,
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
