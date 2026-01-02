namespace Arkanis.Overlay.Host.Server.UnitTests;

using Infrastructure.Data;
using Infrastructure.Data.Extensions;
using Shouldly;
using Xunit.Microsoft.DependencyInjection.Abstracts;

public class ServerHostTest(ITestOutputHelper testOutputHelper, ServerHostTestFixture fixture) : TestBed<ServerHostTestFixture>(testOutputHelper, fixture)
{
    [Fact]
    public async Task Startup_And_Database_Migration()
        => await Should.NotThrowAsync(async () =>
            {
                var serviceProvider = _fixture.GetServiceProvider(_testOutputHelper);
                await serviceProvider.MigrateDatabaseAsync<OverlayDbContext>();
            }
        );
}
