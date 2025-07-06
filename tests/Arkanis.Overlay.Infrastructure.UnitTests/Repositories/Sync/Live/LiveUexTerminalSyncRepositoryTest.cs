namespace Arkanis.Overlay.Infrastructure.UnitTests.Repositories.Sync.Live;

using System.Threading;
using System.Threading.Tasks;
using Domain.Models.Game;
using Xunit;
using Xunit.Abstractions;

/// <remarks>
///     This is the most dependency-complex test.
/// </remarks>
[Trait(nameof(TestConstants.Traits.DataSource), TestConstants.Traits.DataSource.ExternalApi)]
[Trait(nameof(TestConstants.Traits.DataState), TestConstants.Traits.DataState.Live)]
[Collection(TestConstants.Collections.RepositorySyncLiveApi)]
public class LiveUexTerminalSyncRepositoryTest(ITestOutputHelper testOutputHelper, LiveUexSyncRepositoryTestFixture fixture)
    : UexSyncRepositoryTestBase<GameTerminal, LiveUexSyncRepositoryTestFixture>(testOutputHelper, fixture)
{
    protected override async Task LoadDependenciesAsync(CancellationToken cancellationToken)
        => await Task.WhenAll(
                LoadDependenciesAsync<GameStarSystem>(cancellationToken),
                LoadDependenciesAsync<GameMoon>(cancellationToken),
                LoadDependenciesAsync<GamePlanet>(cancellationToken),
                LoadDependenciesAsync<GameCity>(cancellationToken),
                LoadDependenciesAsync<GameOutpost>(cancellationToken),
                LoadDependenciesAsync<GameSpaceStation>(cancellationToken)
            )
            .ConfigureAwait(false);
}
