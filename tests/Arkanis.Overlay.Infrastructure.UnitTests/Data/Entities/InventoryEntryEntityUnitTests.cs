namespace Arkanis.Overlay.Infrastructure.UnitTests.Data.Entities;

using Infrastructure.Data;
using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Shouldly;

[Collection(TestConstants.Collections.DbContext)]
public class InventoryEntryEntityUnitTests(ITestOutputHelper testOutputHelper, OverlayDbContextTestBedFixture fixture)
    : DbContextTestBed<OverlayDbContextTestBedFixture, OverlayDbContext>(testOutputHelper, fixture)
{
    [Theory]
    [MemberData(nameof(DatabaseInventoryEntities))]
    internal async Task Can_Insert_And_Query(InventoryEntryEntityBase sourceItem)
    {
        await using (var dbContext = await CreateDbContextAsync())
        {
            await dbContext.InventoryEntries.AddAsync(sourceItem, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var dbContext = await CreateDbContextAsync())
        {
            var loadedItem = await dbContext.InventoryEntries.SingleAsync(x => x.Id == sourceItem.Id, TestContext.Current.CancellationToken);
            loadedItem.List = sourceItem.List = null; // do not compare equivalence of related entities recursively
            loadedItem.ShouldBeEquivalentTo(sourceItem);
        }
    }

    internal static IEnumerable<InventoryEntryEntityBase[]> DatabaseInventoryEntities()
        =>
        [
            [DatabaseInventoryEntitiesFixture.LocationItem1],
            //
            [DatabaseInventoryEntitiesFixture.PhysicalCommodity1],
        ];
}
