namespace Arkanis.Overlay.Infrastructure.UnitTests.Data;

using Common.UnitTests.Abstractions;
using Common.UnitTests.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

public abstract class DbContextTestBed<TFixture, TContext>
    : TestBed<TFixture>, IDependencyInjectionTestBed
    where TContext : DbContext
    where TFixture : TestBedFixture
{
    protected DbContextTestBed(ITestOutputHelper testOutputHelper, TFixture fixture) : base(testOutputHelper, fixture)
    {
        using var dbContext = CreateDbContext();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    public TestBedFixture Fixture
        => _fixture;

    public ITestOutputHelper OutputHelper
        => _testOutputHelper;

    protected TContext CreateDbContext()
    {
        var dbContextFactory = this.GetRequiredService<IDbContextFactory<TContext>>();
        return dbContextFactory.CreateDbContext();
    }

    protected async Task<TContext> CreateDbContextAsync()
    {
        var dbContextFactory = this.GetRequiredService<IDbContextFactory<TContext>>();
        return await dbContextFactory.CreateDbContextAsync();
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        var dbContext = await CreateDbContextAsync();
        await dbContext.Database.EnsureDeletedAsync();

        await base.DisposeAsyncCore();
    }
}
