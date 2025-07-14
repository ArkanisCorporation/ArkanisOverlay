namespace Arkanis.Overlay.Infrastructure.UnitTests.Repositories.Sync.Live;

using Domain.Models.Game;
using Xunit;
using Xunit.Abstractions;

[Trait(nameof(TestConstants.Traits.DataSource), TestConstants.Traits.DataSource.ExternalApi)]
[Trait(nameof(TestConstants.Traits.DataState), TestConstants.Traits.DataState.Live)]
[Collection(TestConstants.Collections.RepositorySyncLiveApi)]
public class LiveUexCompanySyncRepositoryTest(ITestOutputHelper testOutputHelper, LiveUexSyncRepositoryTestFixture fixture)
    : UexSyncRepositoryTestBase<GameCompany, LiveUexSyncRepositoryTestFixture>(testOutputHelper, fixture);
