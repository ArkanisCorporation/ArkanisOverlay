namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Overlay.Components.Shared;
using Overlay.Domain.Abstractions.Game;
using Overlay.Domain.Abstractions.Services;
using Overlay.Domain.Enums;
using Overlay.Domain.Models.Game;
using Overlay.Domain.Models.Search;
using Shouldly;

public sealed class GameEntitySelectBoxTests : TestContext
{
    public GameEntitySelectBoxTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<ISearchService, EmptySearchService>();
    }

    [Fact]
    public void Selected_entity_is_converted_to_its_full_display_name()
    {
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<GameEntitySelectBox>(parameters => parameters
            .Add(x => x.EntityCategory, GameEntityCategory.Location)
            .Add(x => x.Value, stanton));

        var autocomplete = cut.FindComponent<MudAutocomplete<IGameEntity?>>().Instance;

        autocomplete.Converter.Set(stanton).ShouldBe("Stanton");
    }

    [Fact]
    public void Explicit_clearable_option_allows_a_required_selection_to_be_cleared()
    {
        RenderComponent<MudPopoverProvider>();
        var cut = RenderComponent<GameEntitySelectBox>(parameters => parameters
            .Add(x => x.EntityCategory, GameEntityCategory.Location)
            .Add(x => x.Required, true)
            .Add(x => x.Clearable, true));

        var autocomplete = cut.FindComponent<MudAutocomplete<IGameEntity?>>().Instance;

        autocomplete.Clearable.ShouldBeTrue();
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
