namespace Arkanis.Overlay.Infrastructure.UnitTests.Repositories.Sync.Cached;

using System.Threading;
using System.Threading.Tasks;
using Domain.Models.Game;
using Xunit;
using Xunit.Abstractions;

[Trait(nameof(TestConstants.Traits.DataSource), TestConstants.Traits.DataSource.ExternalApi)]
[Trait(nameof(TestConstants.Traits.DataState), TestConstants.Traits.DataState.Cached)]
[Collection(TestConstants.Collections.RepositorySyncCachedApi)]
public class CachedUexGroundVehicleSyncRepositoryTest(ITestOutputHelper testOutputHelper, CachedUexSyncRepositoryTestFixture fixture)
    : UexSyncRepositoryTestBase<GameGroundVehicle, CachedUexSyncRepositoryTestFixture>(testOutputHelper, fixture)
{
    protected override Task LoadDependenciesAsync(CancellationToken cancellationToken)
        => LoadDependenciesAsync<GameCompany>(cancellationToken);
}
