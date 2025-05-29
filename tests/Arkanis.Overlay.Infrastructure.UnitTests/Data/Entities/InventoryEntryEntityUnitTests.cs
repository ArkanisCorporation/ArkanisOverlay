namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Entities;

using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit.Abstractions;

public class InventoryEntryEntityUnitTests(ITestOutputHelper testOutputHelper, OverlayDbContextTestFixture fixture)
    : DbContextTestBed<OverlayDbContextTestFixture, OverlayDbContext>(testOutputHelper, fixture)
{
    [Fact]
    public async Task Can_Insert_And_Query()
    {
        var sourceItem = DatabaseInventoryEntitiesFixture.PhysicalItem1;

        await using (var dbContext = await CreateDbContextAsync())
        {
            await dbContext.InventoryEntries.AddAsync(sourceItem);
            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = await CreateDbContextAsync())
        {
            var loadedItem = await dbContext.InventoryEntries.SingleAsync();
            loadedItem.ShouldBeEquivalentTo(sourceItem);
        }
    }
}
