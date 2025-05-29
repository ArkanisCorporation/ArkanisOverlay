namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Entities;

using Domain.Models.Inventory;

public static class DomainInventoryEntriesFixture
{
    public static readonly PhysicalItemInventoryEntry PhysicalItem1 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Item = GameEntityFixture.Item1,
        Quantity = new Quantity(1, Quantity.Type.StandardCargoUnit),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly PhysicalItemInventoryEntry PhysicalItem2 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Item = GameEntityFixture.Item2,
        Quantity = new Quantity(2, Quantity.Type.CentiStandardCargoUnit),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly PhysicalItemInventoryEntry PhysicalItem3 = new()
    {
        Id = InventoryEntryId.CreateNew(),
        Item = GameEntityFixture.Item3,
        Quantity = new Quantity(10, Quantity.Type.Count),
        Location = GameEntityFixture.SpaceStation,
    };

    public static readonly InventoryEntryList SourceList = new()
    {
        Id = InventoryEntryListId.CreateNew(),
        Name = nameof(SourceList),
        Notes = "This is for a unit test",
        Entries = [PhysicalItem1, PhysicalItem2],
    };

    static DomainInventoryEntriesFixture()
        => SourceList.Entries.Sort(Comparison);

    public static int Comparison(InventoryEntryBase left, InventoryEntryBase right)
        => left.Id.Identity.CompareTo(right.Id.Identity);
}
