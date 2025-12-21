namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Entities;

using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shouldly;

[Collection(TestConstants.Collections.DbContext)]
public class InventoryEntryListEntityUnitTests(ITestOutputHelper testOutputHelper, OverlayDbContextTestBedFixture fixture)
    : DbContextTestBed<OverlayDbContextTestBedFixture, OverlayDbContext>(testOutputHelper, fixture)
{
    [Fact]
    public async Task Can_Insert_And_Query()
    {
        var sourceList = DatabaseInventoryEntitiesFixture.ItemList;

        await using (var dbContext = await CreateDbContextAsync())
        {
            await dbContext.InventoryLists.AddAsync(sourceList, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var dbContext = await CreateDbContextAsync())
        {
            var loadedItem = await dbContext.InventoryLists.SingleAsync(TestContext.Current.CancellationToken);

            sourceList.Entries.Sort(DatabaseInventoryEntitiesFixture.Comparison);
            loadedItem.ShouldBeEquivalentTo(sourceList);
        }
    }
}
