namespace Arkanis.Overlay.Infrastructure.UnitTests.Data;

using Common.UnitTests.Abstractions;
using Common.UnitTests.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

public abstract class DbContextTestBed<TFixture, TContext>(ITestOutputHelper testOutputHelper, TFixture fixture)
    : TestBed<TFixture>(testOutputHelper, fixture), IDependencyInjectionTestBed
    where TContext : DbContext
    where TFixture : TestBedFixture
{
    public TestBedFixture Fixture
        => _fixture;

    public ITestOutputHelper OutputHelper
        => _testOutputHelper;

    protected TContext CreateDbContext()
    {
        var dbContextFactory = this.GetRequiredService<IDbContextFactory<TContext>>();
        var dbContext = dbContextFactory.CreateDbContext();

        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    protected async Task<TContext> CreateDbContextAsync()
    {
        var dbContextFactory = this.GetRequiredService<IDbContextFactory<TContext>>();
        var dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.Database.EnsureCreatedAsync();
        return dbContext;
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        var dbContext = await CreateDbContextAsync();
        await dbContext.Database.EnsureDeletedAsync();

        await base.DisposeAsyncCore();
    }
}
