namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Entities;

using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit.Abstractions;

[Collection(TestConstants.Collections.DbContext)]
public class InventoryEntryListEntityUnitTests(ITestOutputHelper testOutputHelper, OverlayDbContextTestFixture fixture)
    : DbContextTestBed<OverlayDbContextTestFixture, OverlayDbContext>(testOutputHelper, fixture)
{
    [Fact]
    public async Task Can_Insert_And_Query()
    {
        var sourceList = DatabaseInventoryEntitiesFixture.ItemList;

        await using (var dbContext = await CreateDbContextAsync())
        {
            await dbContext.InventoryLists.AddAsync(sourceList);
            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = await CreateDbContextAsync())
        {
            var loadedItem = await dbContext.InventoryLists
                .Include(x => x.Entries)
                .SingleAsync();

            sourceList.Entries.Sort(DatabaseInventoryEntitiesFixture.Comparison);
            loadedItem.ShouldBeEquivalentTo(sourceList);
        }
    }
}
