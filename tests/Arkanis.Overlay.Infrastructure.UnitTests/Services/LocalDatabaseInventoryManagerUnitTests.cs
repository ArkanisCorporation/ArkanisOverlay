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

public class LocalDatabaseInventoryManagerUnitTests(ITestOutputHelper testOutputHelper, LocalDatabaseServiceTestBedFixture fixture)
    : DbContextTestBed<LocalDatabaseServiceTestBedFixture, OverlayDbContext>(testOutputHelper, fixture)
{
    [Fact]
    public async Task Can_Insert()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var sourceList = new InventoryEntryList
        {
            Name = nameof(Can_Insert),
            Entries = [DomainInventoryEntriesFixture.PhysicalCommodity1],
        };

        await inventoryManager.UpdateListAsync(sourceList);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id);
        databaseList.ShouldBeEquivalentTo(sourceList);
    }

    [Fact]
    public async Task Can_Update_Add_Entries()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var sourceList = new InventoryEntryList
        {
            Name = nameof(Can_Update_Remove_Entries),
            Entries = [DomainInventoryEntriesFixture.PhysicalCommodity1],
        };

        sourceList.Entries.Sort(DomainInventoryEntriesFixture.Comparison);
        await inventoryManager.UpdateListAsync(sourceList);

        sourceList.Entries.Add(DomainInventoryEntriesFixture.PhysicalItem1);
        sourceList.Entries.Sort(DomainInventoryEntriesFixture.Comparison);
        await inventoryManager.UpdateListAsync(sourceList);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id);
        databaseList.ShouldBeEquivalentTo(sourceList);
    }

    [Fact]
    public async Task Can_Update_Remove_Entries()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var sourceList = new InventoryEntryList
        {
            Name = nameof(Can_Update_Remove_Entries),
            Entries =
            [
                DomainInventoryEntriesFixture.PhysicalCommodity1,
                DomainInventoryEntriesFixture.PhysicalItem1,
                DomainInventoryEntriesFixture.PhysicalItem2,
                DomainInventoryEntriesFixture.PhysicalItem3,
            ],
        };

        sourceList.Entries.Sort(DomainInventoryEntriesFixture.Comparison);
        await inventoryManager.UpdateListAsync(sourceList);

        sourceList.Entries.Remove(DomainInventoryEntriesFixture.PhysicalItem1);
        sourceList.Entries.Remove(DomainInventoryEntriesFixture.PhysicalItem2);
        sourceList.Entries.Sort(DomainInventoryEntriesFixture.Comparison);
        await inventoryManager.UpdateListAsync(sourceList);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id);
        databaseList.ShouldBeEquivalentTo(sourceList);
    }

    [Fact]
    public async Task Can_Update_Properties()
    {
        await SetUp();

        var inventoryManager = this.GetRequiredService<LocalDatabaseInventoryManager>();

        var sourceList = new InventoryEntryList
        {
            Name = nameof(Can_Update_Remove_Entries),
            Entries = [DomainInventoryEntriesFixture.PhysicalCommodity1],
        };

        await inventoryManager.UpdateListAsync(sourceList);

        sourceList.Name = "Different name";
        sourceList.Notes = "Different notes";
        await inventoryManager.UpdateListAsync(sourceList);

        var databaseList = await inventoryManager.GetListAsync(sourceList.Id);
        databaseList.ShouldBeEquivalentTo(sourceList);
    }

    private async Task SetUp()
    {
        var uexMapper = this.GetRequiredService<UexApiDtoMapper>();
        GameEntityFixture.AllEntities.ForEach(uexMapper.CacheGameEntity);

        await using var dbContext = await CreateDbContextAsync();
        await dbContext.AddRangeAsync(DatabaseInventoryEntitiesFixture.AllEntries);
    }
}
