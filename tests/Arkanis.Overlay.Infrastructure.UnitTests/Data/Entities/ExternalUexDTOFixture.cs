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

    public static readonly InventoryEntryListEntity SourceList = new()
    {
        Id = InventoryEntryListId.CreateNew(),
        Name = nameof(SourceList),
        Notes = "This is for a unit test",
        Entities = [PhysicalItem1, PhysicalItem2],
    };

    static DatabaseInventoryEntitiesFixture()
        => SourceList.Entities.Sort(Comparison);

    public static int Comparison(InventoryEntryEntityBase left, InventoryEntryEntityBase right)
        => left.Id.Identity.CompareTo(right.Id.Identity);
}
