namespace Arkanis.Overlay.Infrastructure.UnitTests.Repositories.Sync.Cached;

using System.Threading;
using System.Threading.Tasks;
using Domain.Models.Game;
using Xunit;
using Xunit.Abstractions;

[Trait(nameof(TestConstants.Traits.DataSource), TestConstants.Traits.DataSource.ExternalApi)]
[Trait(nameof(TestConstants.Traits.DataState), TestConstants.Traits.DataState.Cached)]
[Collection(TestConstants.Collections.RepositorySyncCachedApi)]
public class CachedUexTerminalSyncRepositoryTest(ITestOutputHelper testOutputHelper, CachedUexSyncRepositoryTestFixture fixture)
    : UexSyncRepositoryTestBase<GameTerminal, CachedUexSyncRepositoryTestFixture>(testOutputHelper, fixture)
{
    protected override Task LoadDependenciesAsync(CancellationToken cancellationToken)
        => Task.WhenAll(
            LoadDependenciesAsync<GameStarSystem>(cancellationToken),
            LoadDependenciesAsync<GameMoon>(cancellationToken),
            LoadDependenciesAsync<GamePlanet>(cancellationToken),
            LoadDependenciesAsync<GameCity>(cancellationToken),
            LoadDependenciesAsync<GameOutpost>(cancellationToken),
            LoadDependenciesAsync<GameSpaceStation>(cancellationToken)
        );
}
