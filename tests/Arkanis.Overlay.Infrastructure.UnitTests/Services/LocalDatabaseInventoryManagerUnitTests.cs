namespace Arkanis.Overlay.Infrastructure.UnitTests.Services;

using Common.UnitTests.Extensions;
using Data;
using Data.Entities;
using Domain.Models.Inventory;
using Infrastructure.Data;
using Infrastructure.Data.Mappers;
using Infrastructure.Services;
using MoreLinq;
using Shouldly;

[Collection(TestConstants.Collections.DbContext)]
public class LocalDatabaseInventoryManagerUnitTests(ITestOutputHelper testOutputHelper, LocalDatabaseServiceTestBedFixture fixture)
    : DbContextTestBed<LocalDatabaseServiceTestBedFixture, OverlayDbContext>(testOutputHelper, fixture)
{
    private void ShouldBeEquivalent(InventoryEntryList? actual, InventoryEntryList? expected)
    {
        if (expected is null)
        {
            actual.ShouldBeNull();
            return;
        }

        actual.ShouldNotBeNull();

        actual.Id.ShouldBe(expected.Id);
        actual.Name.ShouldBe(expected.Name);
        actual.Notes.ShouldBe(expected.Notes);

        actual.Entries.Count.ShouldBe(expected.Entries.Count);

        actual.Entries.Sort(Compare);
        expected.Entries.Sort(Compare);
        foreach (var (actualEntry, expectedEntry) in actual.Entries.Zip(expected.Entries))
        {
            ShouldBeEquivalent(actualEntry, expectedEntry);
        }

        return;

        int Compare(InventoryEntryBase x, InventoryEntryBase y)
            => x.Id.Identity.CompareTo(y.Id.Identity);
    }

    private void ShouldBeEquivalent(InventoryEntryBase actual, InventoryEntryBase expected)
    {
        actual.Id.ShouldBe(expected.Id);
        actual.Type.ShouldBe(expected.Type);
        actual.Quantity.ShouldBeEquivalentTo(expected.Quantity);
        actual.Entity.ShouldBe(expected.Entity);
    }

    [Fact]
    public async Task Can_Insert_Entry()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var source = InventoryEntry.Create(GameEntityFixture.Item1, new Quantity(1, Quantity.UnitType.Count));

        await inventoryManager.AddOrUpdateEntryAsync(source, TestContext.Current.CancellationToken);

        var databaseList = await inventoryManager.GetAllEntriesAsync(TestContext.Current.CancellationToken);
        databaseList.Single(x => x.Id == source.Id).ShouldBeEquivalentTo(source);
    }

    [Fact]
    public async Task Can_Insert_And_Change_Type_Entry()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var source = InventoryEntry.Create(GameEntityFixture.Item2, new Quantity(1, Quantity.UnitType.Count));
        await inventoryManager.AddOrUpdateEntryAsync(source, TestContext.Current.CancellationToken);

        var updatedSource = source.TransferTo(GameEntityFixture.Outpost);
        await inventoryManager.AddOrUpdateEntryAsync(updatedSource, TestContext.Current.CancellationToken);

        var databaseEntries = await inventoryManager.GetAllEntriesAsync(TestContext.Current.CancellationToken);
        var databaseEntry = databaseEntries.SingleOrDefault(x => x.Id == source.Id).ShouldNotBeNull();
        databaseEntry.ShouldBeEquivalentTo(updatedSource);
    }

    [Fact]
    public async Task Can_Get_Vehicle_Cargo_Entry_For_Item()
    {
        // Regression: an item stored in a vehicle's inventory (VehicleInventoryEntry) has a required
        // HangarEntry reference. When queried via GetEntriesForAsync (e.g. from the search details view),
        // the HangarEntry navigation must be loaded, otherwise mapping to the domain model throws
        // ArgumentNullException('HangarEntry') and crashes the Overlay UI.
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var hangarEntry = InventoryEntry.CreateAt(GameEntityFixture.SpaceShip, GameEntityFixture.SpaceStation);
        await inventoryManager.AddOrUpdateEntryAsync(hangarEntry, TestContext.Current.CancellationToken);

        var cargo = InventoryEntry.CreateCargo(GameEntityFixture.Item1, new Quantity(3, Quantity.UnitType.Count), hangarEntry);
        await inventoryManager.AddOrUpdateEntryAsync(cargo, TestContext.Current.CancellationToken);

        var entries = await inventoryManager.GetEntriesForAsync(GameEntityFixture.Item1.Id, TestContext.Current.CancellationToken);

        var cargoEntry = entries.SingleOrDefault(x => x.Id == cargo.Id).ShouldNotBeNull();
        var vehicleEntry = cargoEntry.ShouldBeOfType<VehicleInventoryEntry>();
        vehicleEntry.HangarEntry.Id.ShouldBe(hangarEntry.Id);
    }

    [Fact]
    public async Task Can_Get_Hangar_With_Its_Vehicle_Cargo()
    {
        // Complements the search path above: when a hangar is loaded as a top-level entry (as the hangar
        // view does via GetAllEntriesAsync) its Inventory/Modules must be fully populated. Mapping this
        // hangar<->cargo reference cycle must terminate (the cargo's HangarEntry is mapped without walking
        // back into the hangar's collections).
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var hangarEntry = InventoryEntry.CreateAt(GameEntityFixture.SpaceShip, GameEntityFixture.SpaceStation);
        await inventoryManager.AddOrUpdateEntryAsync(hangarEntry, TestContext.Current.CancellationToken);

        var cargo = InventoryEntry.CreateCargo(GameEntityFixture.Item1, new Quantity(3, Quantity.UnitType.Count), hangarEntry);
        await inventoryManager.AddOrUpdateEntryAsync(cargo, TestContext.Current.CancellationToken);

        var allEntries = await inventoryManager.GetAllEntriesAsync(TestContext.Current.CancellationToken);

        var loadedHangar = allEntries.OfType<HangarInventoryEntry>().SingleOrDefault(x => x.Id == hangarEntry.Id).ShouldNotBeNull();
        loadedHangar.Inventory.ShouldContain(x => x.Id == cargo.Id);
    }

    [Fact]
    public async Task Can_Insert_List()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var sourceList = new InventoryEntryList
        {
            Name = nameof(Can_Insert_List),
            Entries = [DomainInventoryEntriesFixture.LocationCommodity1],
        };

        await inventoryManager.AddOrUpdateListAsync(sourceList, TestContext.Current.CancellationToken);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id, TestContext.Current.CancellationToken);
        ShouldBeEquivalent(databaseList, sourceList);
    }

    [Fact]
    public async Task Can_Update_List_Add_Entries()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var sourceList = new InventoryEntryList
        {
            Name = nameof(Can_Update_List_Remove_Entries),
            Entries = [DomainInventoryEntriesFixture.LocationCommodity1],
        };

        sourceList.Entries.Sort(DomainInventoryEntriesFixture.Comparison);
        await inventoryManager.AddOrUpdateListAsync(sourceList, TestContext.Current.CancellationToken);

        sourceList.Entries.Add(DomainInventoryEntriesFixture.LocationItem1);
        sourceList.Entries.Sort(DomainInventoryEntriesFixture.Comparison);
        await inventoryManager.AddOrUpdateListAsync(sourceList, TestContext.Current.CancellationToken);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id, TestContext.Current.CancellationToken);
        ShouldBeEquivalent(databaseList, sourceList);
    }

    [Fact]
    public async Task Can_Update_List_Remove_Entries()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var sourceList = new InventoryEntryList
        {
            Name = nameof(Can_Update_List_Remove_Entries),
            Entries =
            [
                DomainInventoryEntriesFixture.LocationCommodity1,
                DomainInventoryEntriesFixture.LocationItem1,
                DomainInventoryEntriesFixture.LocationItem2,
                DomainInventoryEntriesFixture.LocationItem3,
            ],
        };

        sourceList.Entries.Sort(DomainInventoryEntriesFixture.Comparison);
        await inventoryManager.AddOrUpdateListAsync(sourceList, TestContext.Current.CancellationToken);

        sourceList.Entries.Remove(DomainInventoryEntriesFixture.LocationItem1);
        sourceList.Entries.Remove(DomainInventoryEntriesFixture.LocationItem2);
        sourceList.Entries.Sort(DomainInventoryEntriesFixture.Comparison);
        await inventoryManager.AddOrUpdateListAsync(sourceList, TestContext.Current.CancellationToken);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id, TestContext.Current.CancellationToken);
        ShouldBeEquivalent(databaseList, sourceList);
    }

    [Fact]
    public async Task Can_Update_List()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var sourceList = new InventoryEntryList
        {
            Name = nameof(Can_Update_List),
            Entries = [DomainInventoryEntriesFixture.LocationCommodity1],
        };

        await inventoryManager.AddOrUpdateListAsync(sourceList, TestContext.Current.CancellationToken);

        sourceList.Name = "Different name";
        sourceList.Notes = "Different notes";
        await inventoryManager.AddOrUpdateListAsync(sourceList, TestContext.Current.CancellationToken);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id, TestContext.Current.CancellationToken);
        ShouldBeEquivalent(databaseList, sourceList);
    }

    [Fact]
    public async Task Can_Update_Entry_Quantity()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();
        var source = InventoryEntry.Create(GameEntityFixture.Item1, new Quantity(1, Quantity.UnitType.Count));

        await inventoryManager.AddOrUpdateEntryAsync(source, TestContext.Current.CancellationToken);

        source.Quantity = source.Quantity with
        {
            Amount = 6,
        };
        await inventoryManager.AddOrUpdateEntryAsync(source, TestContext.Current.CancellationToken);

        var dbEntry = (await inventoryManager.GetAllEntriesAsync(TestContext.Current.CancellationToken)).Single(x => x.Id == source.Id);
        dbEntry.Quantity.ShouldBeEquivalentTo(source.Quantity);
    }

    [Fact]
    public async Task Can_Change_Location_Entry_Location()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();
        var source = InventoryEntry.Create(GameEntityFixture.Item2, new Quantity(1, Quantity.UnitType.Count)).TransferTo(GameEntityFixture.Outpost);

        await inventoryManager.AddOrUpdateEntryAsync(source, TestContext.Current.CancellationToken);

        var updated = source.TransferTo(GameEntityFixture.City);
        await inventoryManager.AddOrUpdateEntryAsync(updated, TestContext.Current.CancellationToken);

        var dbEntry = (await inventoryManager.GetAllEntriesAsync(TestContext.Current.CancellationToken)).Single(x => x.Id == source.Id);
        dbEntry.ShouldBeEquivalentTo(updated);
    }

    [Fact]
    public async Task Can_Remove_Entry()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();
        var source = InventoryEntry.Create(GameEntityFixture.Item1, new Quantity(1, Quantity.UnitType.Count));

        await inventoryManager.AddOrUpdateEntryAsync(source, TestContext.Current.CancellationToken);

        await inventoryManager.DeleteEntryAsync(source.Id, TestContext.Current.CancellationToken);

        var dbEntries = await inventoryManager.GetAllEntriesAsync(TestContext.Current.CancellationToken);
        dbEntries.ShouldNotContain(x => x.Id == source.Id);
    }

    [Fact]
    public async Task Remove_NonExistent_Entry_Does_Not_Throw()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();
        var nonExistentId = InventoryEntryId.CreateNew();

        await Should.NotThrowAsync(async () => await inventoryManager.DeleteEntryAsync(nonExistentId));
    }

    [Fact]
    public async Task Update_NonExistent_Entry_Inserts_It()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();
        var source = InventoryEntry.Create(GameEntityFixture.Item3, new Quantity(2, Quantity.UnitType.Count));

        await inventoryManager.AddOrUpdateEntryAsync(source, TestContext.Current.CancellationToken);

        var dbEntry = (await inventoryManager.GetAllEntriesAsync(TestContext.Current.CancellationToken)).SingleOrDefault(x => x.Id == source.Id);
        dbEntry.ShouldNotBeNull();
        dbEntry.ShouldBeEquivalentTo(source);
    }

    private async Task SetUp()
    {
        var uexMapper = this.GetRequiredService<UexApiDtoMapper>();
        GameEntityFixture.AllEntities.ForEach(uexMapper.CacheGameEntity);

        await using var dbContext = await CreateDbContextAsync();
        await dbContext.AddRangeAsync(DatabaseInventoryEntitiesFixture.AllEntries);
    }
}
