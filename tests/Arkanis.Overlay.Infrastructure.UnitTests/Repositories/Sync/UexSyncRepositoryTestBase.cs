namespace Arkanis.Overlay.Infrastructure.UnitTests.Repositories.Sync;

using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models;
using Domain.Models.Game;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

public abstract class UexSyncRepositoryTestBase<TEntity, TFixture>(ITestOutputHelper testOutputHelper, TFixture fixture)
    : TestBed<TFixture>(testOutputHelper, fixture)
    where TEntity : GameEntity where TFixture : TestBedFixture
{
    protected CancellationTokenSource TestCancellation { get; } = new(TimeSpan.FromSeconds(10));

    protected CancellationToken TestCancellationToken
        => TestCancellation.Token;

    protected IGameEntityRepository<TOtherEntity> ResolveRepositoryFor<TOtherEntity>()
        where TOtherEntity : class, IGameEntity
        => _fixture.GetService<IGameEntityRepository<TOtherEntity>>(_testOutputHelper).ShouldNotBeNull();

    protected IGameEntityExternalSyncRepository<TEntity> ResolveSyncRepositoryFor()
        => _fixture.GetService<IGameEntityExternalSyncRepository<TEntity>>(_testOutputHelper).ShouldNotBeNull();

    [Fact]
    public async Task GetAllAsync_Should_Correctly_Map_Items()
    {
        var repositorySUT = ResolveSyncRepositoryFor();

        await LoadDependenciesAsync(TestCancellationToken);
        _fixture.GetService<ILogger<UexSyncRepositoryTestBase<TEntity, TFixture>>>(_testOutputHelper)?.LogDebug("Dependencies loaded");

        var syncData = await repositorySUT.GetAllAsync(DataMissing.Instance, TestCancellationToken);

        var loadedSyncData = syncData.ShouldBeOfType<LoadedSyncData<TEntity>>();

        var entities = await loadedSyncData.GameEntities.ToListAsync(TestCancellationToken);
        entities.ShouldNotBeEmpty();
    }

    protected async Task LoadDependenciesAsync<TOtherEntity>(CancellationToken cancellationToken) where TOtherEntity : class, IGameEntity
    {
        var planetRepository = ResolveRepositoryFor<TOtherEntity>();
        var syncData = new LoadedSyncData<TOtherEntity>(planetRepository.GetAllAsync(cancellationToken), TestFixtures.DataCached);
        await planetRepository.UpdateAllAsync(syncData, cancellationToken);
    }

    protected virtual Task LoadDependenciesAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
