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
using Xunit.Abstractions;

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

        await inventoryManager.AddOrUpdateEntryAsync(source);

        var databaseList = await inventoryManager.GetAllEntriesAsync();
        databaseList.Single(x => x.Id == source.Id).ShouldBeEquivalentTo(source);
    }

    [Fact]
    public async Task Can_Insert_And_Change_Type_Entry()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var source = InventoryEntry.Create(GameEntityFixture.Item2, new Quantity(1, Quantity.UnitType.Count));
        await inventoryManager.AddOrUpdateEntryAsync(source);

        var updatedSource = source.SetLocation(GameEntityFixture.Outpost);
        await inventoryManager.AddOrUpdateEntryAsync(updatedSource);

        var databaseEntries = await inventoryManager.GetAllEntriesAsync();
        var databaseEntry = databaseEntries.SingleOrDefault(x => x.Id == source.Id).ShouldNotBeNull();
        databaseEntry.ShouldBeEquivalentTo(updatedSource);
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

        await inventoryManager.AddOrUpdateListAsync(sourceList);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id);
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
        await inventoryManager.AddOrUpdateListAsync(sourceList);

        sourceList.Entries.Add(DomainInventoryEntriesFixture.LocationItem1);
        sourceList.Entries.Sort(DomainInventoryEntriesFixture.Comparison);
        await inventoryManager.AddOrUpdateListAsync(sourceList);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id);
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
        await inventoryManager.AddOrUpdateListAsync(sourceList);

        sourceList.Entries.Remove(DomainInventoryEntriesFixture.LocationItem1);
        sourceList.Entries.Remove(DomainInventoryEntriesFixture.LocationItem2);
        sourceList.Entries.Sort(DomainInventoryEntriesFixture.Comparison);
        await inventoryManager.AddOrUpdateListAsync(sourceList);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id);
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

        await inventoryManager.AddOrUpdateListAsync(sourceList);

        sourceList.Name = "Different name";
        sourceList.Notes = "Different notes";
        await inventoryManager.AddOrUpdateListAsync(sourceList);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id);
        ShouldBeEquivalent(databaseList, sourceList);
    }

    [Fact]
    public async Task Can_Update_Entry_Quantity()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();
        var source = InventoryEntry.Create(GameEntityFixture.Item1, new Quantity(1, Quantity.UnitType.Count));

        await inventoryManager.AddOrUpdateEntryAsync(source);

        source.Quantity = source.Quantity with
        {
            Amount = 6,
        };
        await inventoryManager.AddOrUpdateEntryAsync(source);

        var dbEntry = (await inventoryManager.GetAllEntriesAsync()).Single(x => x.Id == source.Id);
        dbEntry.Quantity.ShouldBeEquivalentTo(source.Quantity);
    }

    [Fact]
    public async Task Can_Change_Location_Entry_Location()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();
        var source = InventoryEntry.Create(GameEntityFixture.Item2, new Quantity(1, Quantity.UnitType.Count)).SetLocation(GameEntityFixture.Outpost);

        await inventoryManager.AddOrUpdateEntryAsync(source);

        var updated = source.SetLocation(GameEntityFixture.City);
        await inventoryManager.AddOrUpdateEntryAsync(updated);

        var dbEntry = (await inventoryManager.GetAllEntriesAsync()).Single(x => x.Id == source.Id);
        dbEntry.ShouldBeEquivalentTo(updated);
    }

    [Fact]
    public async Task Can_Remove_Entry()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();
        var source = InventoryEntry.Create(GameEntityFixture.Item1, new Quantity(1, Quantity.UnitType.Count));

        await inventoryManager.AddOrUpdateEntryAsync(source);

        await inventoryManager.DeleteEntryAsync(source.Id);

        var dbEntries = await inventoryManager.GetAllEntriesAsync();
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

        await inventoryManager.AddOrUpdateEntryAsync(source);

        var dbEntry = (await inventoryManager.GetAllEntriesAsync()).SingleOrDefault(x => x.Id == source.Id);
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
