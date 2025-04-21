namespace Arkanis.Overlay.Infrastructure.UnitTests.Repositories.Sync.Live;

using Domain.Models.Game;
using Xunit.Abstractions;

/// <remarks>
///     This is the most dependency-complex test.
/// </remarks>
[Collection("Live UEX API")]
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
