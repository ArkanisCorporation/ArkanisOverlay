namespace Arkanis.Overlay.Infrastructure.UnitTests.Repositories.Sync.Live;

using Domain.Models.Game;
using Xunit;
using Xunit.Abstractions;

[Trait(nameof(TestConstants.Traits.DataSource), TestConstants.Traits.DataSource.ExternalApi)]
[Trait(nameof(TestConstants.Traits.DataState), TestConstants.Traits.DataState.Live)]
[Collection(TestConstants.Collections.RepositorySyncLiveApi)]
public class LiveUexCommoditySyncRepositoryTest(ITestOutputHelper testOutputHelper, LiveUexSyncRepositoryTestFixture fixture)
    : UexSyncRepositoryTestBase<GameCommodity, LiveUexSyncRepositoryTestFixture>(testOutputHelper, fixture);
