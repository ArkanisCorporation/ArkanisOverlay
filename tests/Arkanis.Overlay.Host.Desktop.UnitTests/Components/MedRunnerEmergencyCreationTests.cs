namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using Overlay.Components.Shared;
using Overlay.Domain.Abstractions.Game;
using Overlay.Domain.Abstractions.Services;
using Overlay.Domain.Enums;
using Overlay.Domain.Models.Game;
using Overlay.Domain.Models.Search;
using Overlay.External.MedRunner;
using Overlay.Infrastructure.Services;
using Overlay.Infrastructure.Services.External;
using Shouldly;

public sealed class MedRunnerEmergencyCreationTests : TestContext
{
    public MedRunnerEmergencyCreationTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<ISearchService, EmptySearchService>();
        Services.AddMockMedRunnerApiClient();
        Services.AddSingleton<IUserPreferencesManager, InMemoryUserPreferencesManager>();
        Services.AddLogging();
        Services.AddSingleton<MedRunnerAccountContext>();
    }

    [Fact]
    public async Task Changing_a_system_clears_dependent_location_selections_and_keeps_alternatives_scoped()
    {
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var pyro = new GameStarSystem(2, "Pyro", "PYR");
        var microTech = new GamePlanet(3, "microTech", "MIC", stanton);
        var newBabbage = new GameCity(4, "New Babbage", "NBB", microTech);
        var monox = new GamePlanet(5, "Monox", "MNX", pyro);
        RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<MedRunnerEmergencyCreation>(parameters => parameters
            .Add(x => x.EmergencyContext, new MedRunnerComponentBase.EmergencyContextModel()));
        var selections = cut.FindComponents<GameEntitySelectBox>();

        await cut.InvokeAsync(() => selections[0].Instance.ValueChanged.InvokeAsync(stanton));
        await cut.InvokeAsync(() => selections[1].Instance.ValueChanged.InvokeAsync(microTech));
        await cut.InvokeAsync(() => selections[2].Instance.ValueChanged.InvokeAsync(newBabbage));
        await cut.InvokeAsync(() => selections[0].Instance.ValueChanged.InvokeAsync(pyro));

        selections[0].Instance.Value.ShouldBe(pyro);
        selections[1].Instance.Value.ShouldBeNull();
        selections[2].Instance.Value.ShouldBeNull();
        selections[1].Instance.Accept(monox).ShouldBeTrue();
        selections[1].Instance.Accept(microTech).ShouldBeFalse();
    }

    [Fact]
    public async Task Changing_a_planet_or_moon_clears_the_closest_location_selection()
    {
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var microTech = new GamePlanet(2, "microTech", "MIC", stanton);
        var hurston = new GamePlanet(3, "Hurston", "HUR", stanton);
        var newBabbage = new GameCity(4, "New Babbage", "NBB", microTech);
        RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<MedRunnerEmergencyCreation>(parameters => parameters
            .Add(x => x.EmergencyContext, new MedRunnerComponentBase.EmergencyContextModel()));
        var selections = cut.FindComponents<GameEntitySelectBox>();

        await cut.InvokeAsync(() => selections[0].Instance.ValueChanged.InvokeAsync(stanton));
        await cut.InvokeAsync(() => selections[1].Instance.ValueChanged.InvokeAsync(microTech));
        await cut.InvokeAsync(() => selections[2].Instance.ValueChanged.InvokeAsync(newBabbage));
        await cut.InvokeAsync(() => selections[1].Instance.ValueChanged.InvokeAsync(hurston));

        selections[1].Instance.Value.ShouldBe(hurston);
        selections[2].Instance.Value.ShouldBeNull();
        selections[2].Instance.Accept(newBabbage).ShouldBeFalse();
    }

    private sealed class EmptySearchService : ISearchService
    {
        public async IAsyncEnumerable<string> GetSearchTokensAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        public Task<GameEntitySearchResults> SearchAsync(IEnumerable<SearchQuery> queries, CancellationToken cancellationToken = default)
            => Task.FromResult(new GameEntitySearchResults([], TimeSpan.Zero));
    }
}
