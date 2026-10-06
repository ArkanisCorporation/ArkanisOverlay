namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using global::Arkanis.Overlay.Common.Abstractions;

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
using Overlay.Common.Models;
using Overlay.Common.Options;
using Overlay.External.MedRunner.API;
using Overlay.External.MedRunner.API.Abstractions.Endpoints;
using Overlay.External.MedRunner.API.Endpoints.Emergency.Request;
using Overlay.External.MedRunner.API.Endpoints.Emergency.Response;
using Overlay.External.MedRunner.API.Mocks.Endpoints;
using Overlay.External.MedRunner.Models;
using Overlay.Infrastructure.Services;
using Overlay.Infrastructure.Services.External;
using Shouldly;

public sealed class MedRunnerEmergencyCreationTests : BunitContext
{
    public MedRunnerEmergencyCreationTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<ISearchService, EmptySearchService>();
        Services.AddMockMedRunnerApiClient();
        Services.AddSingleton<IUserPreferencesManager, InMemoryUserPreferencesManager>();
        Services.AddLogging();
        Services.AddMemoryCache();
        Services.Configure<MedRunnerIntegrationOptions>(options => options.AccountLinkingEnabled = true);
        Services.AddSingleton<RecordingEmergencyEndpoint>();
        Services.AddSingleton<IEmergencyEndpoint>(provider => provider.GetRequiredService<RecordingEmergencyEndpoint>());
        Services.AddSingleton<MedRunnerAccountContext>();
    }

    [Fact]
    public async Task Location_pickers_require_enabled_matching_systems_and_planets()
    {
        await ConfigureAccountAsync();
        var stanton = new GameStarSystem(1, "STANTON", "STA");
        var pyro = new GameStarSystem(2, "Pyro", "PYR");
        var crusader = new GamePlanet(3, "crusader", "CRU", stanton);
        var hurston = new GamePlanet(4, "Hurston", "HUR", stanton);
        var daymar = new GameMoon(5, "Daymar", "DAY", crusader);
        var cut = RenderCreation();
        var selections = cut.FindComponents<GameEntitySelectBox>();

        selections[0].Instance.Accept(stanton).ShouldBeTrue();
        selections[0].Instance.Accept(pyro).ShouldBeFalse();
        await cut.InvokeAsync(() => selections[0].Instance.ValueChanged.InvokeAsync(stanton));
        selections[1].Instance.Accept(crusader).ShouldBeTrue();
        selections[1].Instance.Accept(daymar).ShouldBeTrue();
        selections[1].Instance.Accept(hurston).ShouldBeFalse();

        Services.GetRequiredService<MockOrgSettingsEndpoint>().PublicSettings.LocationSettings.Locations[0].Enabled = false;
        selections[0].Instance.Accept(stanton).ShouldBeFalse();
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, "Daymar")]
    public async Task Submission_uses_settings_spelling_and_preserves_a_selected_moon(bool selectMoon, string? tertiaryLocation)
    {
        await ConfigureAccountAsync();
        var stanton = new GameStarSystem(1, "STANTON", "STA");
        var crusader = new GamePlanet(2, "crusader", "CRU", stanton);
        var daymar = new GameMoon(3, "DAYMAR", "DAY", crusader);
        var cut = RenderCreation();
        await SelectLocationAsync(cut, stanton, selectMoon ? daymar : crusader);

        await SubmitAsync(cut);

        var request = Services.GetRequiredService<RecordingEmergencyEndpoint>().Requests.ShouldHaveSingleItem();
        request.Location.System.ShouldBe("Stanton");
        request.Location.Subsystem.ShouldBe("Crusader");
        request.Location.TertiaryLocation.ShouldBe(tertiaryLocation);
    }

    [Fact]
    public async Task Unsupported_outpost_is_preserved_in_remarks_while_the_request_uses_its_supported_moon()
    {
        await ConfigureAccountAsync();
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var crusader = new GamePlanet(2, "Crusader", "CRU", stanton);
        var daymar = new GameMoon(3, "Daymar", "DAY", crusader);
        var outpost = new GameOutpost(4, "Bountiful Harvest", "BH", daymar);
        var cut = RenderCreation();
        await SelectLocationAsync(cut, stanton, daymar, outpost);
        cut.FindComponents<GameEntitySelectBox>()[2].Instance.Accept(outpost).ShouldBeTrue();

        await SubmitAsync(cut);

        var request = Services.GetRequiredService<RecordingEmergencyEndpoint>().Requests.ShouldHaveSingleItem();
        request.Location.TertiaryLocation.ShouldBe("Daymar");
        request.Remarks.ShouldNotBeNull().ShouldContain("Stanton / Crusader / Daymar / Bountiful Harvest");
    }

    [Fact]
    public async Task Submission_is_revalidated_if_settings_change_while_entering_the_loading_state()
    {
        await ConfigureAccountAsync();
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var crusader = new GamePlanet(2, "Crusader", "CRU", stanton);
        var cut = Render<MedRunnerEmergencyCreation>(parameters => parameters
            .Add(x => x.EmergencyContext, new MedRunnerComponentBase.EmergencyContextModel())
            .Add(x => x.IsLoadingChanged, EventCallback.Factory.Create<bool>(this, loading =>
            {
                if (loading)
                {
                    Services.GetRequiredService<MockOrgSettingsEndpoint>().PublicSettings.LocationSettings.Locations[0].Children[0].Enabled = false;
                }
            })));
        await SelectLocationAsync(cut, stanton, crusader);

        await SubmitAsync(cut);

        Services.GetRequiredService<RecordingEmergencyEndpoint>().Requests.ShouldBeEmpty();
        cut.Markup.ShouldContain("location");
    }

    private IRenderedComponent<MedRunnerEmergencyCreation> RenderCreation()
        => Render<MedRunnerEmergencyCreation>(parameters => parameters
            .Add(x => x.EmergencyContext, new MedRunnerComponentBase.EmergencyContextModel()));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Unconfigured_or_disabled_moon_is_kept_as_detail_without_sending_an_invalid_tertiary(bool disabled)
    {
        await ConfigureAccountAsync();
        var planetSettings = Services.GetRequiredService<MockOrgSettingsEndpoint>().PublicSettings.LocationSettings.Locations[0].Children[0];
        if (disabled)
        {
            planetSettings.Children[0].Enabled = false;
        }
        else
        {
            planetSettings.Children.Clear();
        }
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var crusader = new GamePlanet(2, "Crusader", "CRU", stanton);
        var daymar = new GameMoon(3, "Daymar", "DAY", crusader);
        var cut = RenderCreation();
        await SelectLocationAsync(cut, stanton, daymar);
        cut.FindComponents<GameEntitySelectBox>()[1].Instance.Accept(daymar).ShouldBeTrue();

        await SubmitAsync(cut);

        var request = Services.GetRequiredService<RecordingEmergencyEndpoint>().Requests.ShouldHaveSingleItem();
        request.Location.TertiaryLocation.ShouldBeNull();
        request.Remarks.ShouldNotBeNull().ShouldContain("Stanton / Crusader / Daymar");
    }

    [Fact]
    public async Task Deepest_supported_location_is_used_instead_of_its_supported_ancestor()
    {
        await ConfigureAccountAsync();
        Services.GetRequiredService<MockOrgSettingsEndpoint>().PublicSettings.LocationSettings.Locations[0].Children[0].Children
            .Add(new SpaceLocation { Name = "Bountiful Harvest", Enabled = true, Children = [] });
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var crusader = new GamePlanet(2, "Crusader", "CRU", stanton);
        var daymar = new GameMoon(3, "Daymar", "DAY", crusader);
        var outpost = new GameOutpost(4, "Bountiful Harvest", "BH", daymar);
        var cut = RenderCreation();
        await SelectLocationAsync(cut, stanton, daymar, outpost);

        await SubmitAsync(cut);

        var request = Services.GetRequiredService<RecordingEmergencyEndpoint>().Requests.ShouldHaveSingleItem();
        request.Location.TertiaryLocation.ShouldBe("Bountiful Harvest");
        request.Remarks.ShouldNotBeNull().ShouldContain("Stanton / Crusader / Daymar / Bountiful Harvest");
    }

    [Fact]
    public async Task Creation_prefills_the_location_details_form_and_preserves_user_edits()
    {
        await ConfigureAccountAsync();
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var crusader = new GamePlanet(2, "Crusader", "CRU", stanton);
        var daymar = new GameMoon(3, "Daymar", "DAY", crusader);
        var outpost = new GameOutpost(4, "Bountiful Harvest", "BH", daymar);
        var creation = RenderCreation();
        await SelectLocationAsync(creation, stanton, daymar, outpost);
        await SubmitAsync(creation);
        var context = creation.Instance.EmergencyContext;
        Services.GetRequiredService<MockChatMessageEndpoint>().ChatMessages["offline-emergency"] =
        [new ChatMessage { Id = "situation", EmergencyId = "offline-emergency", SenderId = "offline-client", Content = "## Emergency details: Situation\n\nStranded" }];

        var details = Render<MedRunnerEmergencyDetails>(parameters => parameters.Add(x => x.EmergencyContext, context));

        details.WaitForAssertion(() => details.FindComponents<MudTextField<string>>().Single(field => field.Instance.Label == "Exact location")
            .Instance.Value.ShouldBe("Stanton / Crusader / Daymar / Bountiful Harvest"));
        details.FindComponents<MudSelect<string>>().Single(field => field.Instance.Label == "Location type").Instance.Value.ShouldBe("Outpost");
        await details.InvokeAsync(() => details.FindComponents<MudTextField<string>>().Single(field => field.Instance.Label == "Exact location")
            .Instance.ValueChanged.InvokeAsync("Inside the storage building"));
        await details.InvokeAsync(() => details.FindComponents<MudSelect<string>>().Single(field => field.Instance.Label == "Location type")
            .Instance.ValueChanged.InvokeAsync("Other"));
        details.Render(parameters => parameters.Add(x => x.EmergencyContext, context));
        details.FindComponents<MudTextField<string>>().Single(field => field.Instance.Label == "Exact location")
            .Instance.Value.ShouldBe("Inside the storage building");
        details.FindComponents<MudSelect<string>>().Single(field => field.Instance.Label == "Location type").Instance.Value.ShouldBe("Other");
    }

    [Fact]
    public async Task Creation_prefills_remarks_when_restoring_the_remarks_step()
    {
        await ConfigureAccountAsync();
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var crusader = new GamePlanet(2, "Crusader", "CRU", stanton);
        var city = new GameCity(3, "Orison", "ORI", crusader);
        var creation = RenderCreation();
        await SelectLocationAsync(creation, stanton, crusader, city);
        await SubmitAsync(creation);
        Services.GetRequiredService<MockChatMessageEndpoint>().ChatMessages["offline-emergency"] =
        [
            new ChatMessage { Id = "situation", EmergencyId = "offline-emergency", SenderId = "offline-client", Content = "## Emergency details: Situation\n\nStranded" },
            new ChatMessage { Id = "location", EmergencyId = "offline-emergency", SenderId = "offline-client", Content = "## Emergency details: Location\n\nOrison" },
            new ChatMessage { Id = "players", EmergencyId = "offline-emergency", SenderId = "offline-client", Content = "## Emergency details: Players\n\nNone" },
        ];

        var details = Render<MedRunnerEmergencyDetails>(parameters => parameters.Add(x => x.EmergencyContext, creation.Instance.EmergencyContext));

        details.WaitForAssertion(() => details.FindComponent<MudTextField<string>>().Instance.Value.ShouldNotBeNull().ShouldContain("Stanton / Crusader / Orison"));
        await details.InvokeAsync(() => details.FindComponent<MudTextField<string>>().Instance.ValueChanged.InvokeAsync("Waiting beside the elevators"));
        details.Render(parameters => parameters.Add(x => x.EmergencyContext, creation.Instance.EmergencyContext));
        details.FindComponent<MudTextField<string>>().Instance.Value.ShouldBe("Waiting beside the elevators");
    }

    [Fact]
    public async Task Empty_or_disabled_settings_block_submission_even_for_a_previously_selected_location()
    {
        await ConfigureAccountAsync();
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var crusader = new GamePlanet(2, "Crusader", "CRU", stanton);
        var cut = RenderCreation();
        await SelectLocationAsync(cut, stanton, crusader);
        Services.GetRequiredService<MockOrgSettingsEndpoint>().PublicSettings.LocationSettings.Locations.Clear();

        await SubmitAsync(cut);

        Services.GetRequiredService<RecordingEmergencyEndpoint>().Requests.ShouldBeEmpty();
        cut.FindComponents<GameEntitySelectBox>()[0].Instance.Accept(stanton).ShouldBeFalse();
    }

    [Fact]
    public async Task A_location_from_another_planet_cannot_supply_the_tertiary_name()
    {
        await ConfigureAccountAsync();
        var settings = Services.GetRequiredService<MockOrgSettingsEndpoint>().PublicSettings.LocationSettings;
        settings.Locations[0].Children[0].Children.Clear();
        settings.Locations[0].Children[1].Enabled = true;
        settings.Locations[0].Children[1].Children.Add(new SpaceLocation { Name = "Daymar", Enabled = true, Children = [] });
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var crusader = new GamePlanet(2, "Crusader", "CRU", stanton);
        var daymar = new GameMoon(3, "Daymar", "DAY", crusader);
        var cut = RenderCreation();
        await SelectLocationAsync(cut, stanton, daymar);

        await SubmitAsync(cut);

        var request = Services.GetRequiredService<RecordingEmergencyEndpoint>().Requests.ShouldHaveSingleItem();
        request.Location.Subsystem.ShouldBe("Crusader");
        request.Location.TertiaryLocation.ShouldBeNull();
    }

    [Fact]
    public async Task Details_for_a_different_emergency_do_not_reuse_the_previous_location_draft()
    {
        await ConfigureAccountAsync();
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var crusader = new GamePlanet(2, "Crusader", "CRU", stanton);
        var creation = RenderCreation();
        await SelectLocationAsync(creation, stanton, crusader, new GameCity(3, "Orison", "ORI", crusader));
        await SubmitAsync(creation);
        var context = creation.Instance.EmergencyContext;
        Services.GetRequiredService<MockChatMessageEndpoint>().ChatMessages["offline-emergency"] =
        [new ChatMessage { Id = "situation", EmergencyId = "offline-emergency", SenderId = "offline-client", Content = "## Emergency details: Situation\n\nStranded" }];
        var details = Render<MedRunnerEmergencyDetails>(parameters => parameters.Add(x => x.EmergencyContext, context));
        details.WaitForAssertion(() => details.FindComponents<MudTextField<string>>().Single(field => field.Instance.Label == "Exact location")
            .Instance.Value.ShouldBe("Stanton / Crusader / Orison"));
        await details.InvokeAsync(() => details.FindComponents<MudTextField<string>>().Single(field => field.Instance.Label == "Exact location")
            .Instance.ValueChanged.InvokeAsync("Inside the previous emergency's building"));
        await details.InvokeAsync(() => details.FindComponents<MudSelect<string>>().Single(field => field.Instance.Label == "Location type")
            .Instance.ValueChanged.InvokeAsync("Outpost"));
        await ContinueDetailsAsync(details);
        await ContinueDetailsAsync(details);
        await details.InvokeAsync(() => details.FindComponent<MudTextField<string>>().Instance.ValueChanged.InvokeAsync("Previous emergency remarks"));

        context.Emergency!.Id = "another-emergency";
        context.Emergency.System = "Pyro";
        context.Emergency.Subsystem = "Monox";
        context.Emergency.Remarks = "Wait near the landing pad";
        Services.GetRequiredService<MockChatMessageEndpoint>().ChatMessages["another-emergency"] =
        [new ChatMessage { Id = "situation", EmergencyId = "another-emergency", SenderId = "offline-client", Content = "## Emergency details: Situation\n\nStranded" }];

        details.Render(parameters => parameters.Add(x => x.EmergencyContext, context));

        details.WaitForAssertion(() => details.FindComponents<MudTextField<string>>().Single(field => field.Instance.Label == "Exact location")
            .Instance.Value.ShouldBe("Pyro / Monox"));
        details.FindComponents<MudSelect<string>>().Single(field => field.Instance.Label == "Location type").Instance.Value.ShouldBeEmpty();
        await ContinueDetailsAsync(details);
        await ContinueDetailsAsync(details);
        details.FindComponent<MudTextField<string>>().Instance.Value.ShouldBe("Wait near the landing pad");
    }

    private async Task ConfigureAccountAsync()
    {
        Render<MudPopoverProvider>();
        Services.GetRequiredService<MockOrgSettingsEndpoint>().PublicSettings.LocationSettings.Locations =
        [
            new SpaceLocation
            {
                Name = "Stanton", Enabled = true, Type = SpaceLocationType.System,
                Children =
                [
                    new SpaceLocation
                    {
                        Name = "Crusader", Enabled = true, Type = SpaceLocationType.Planet,
                        Children = [new SpaceLocation { Name = "Daymar", Enabled = true, Type = SpaceLocationType.Moon, Children = [] }],
                    },
                    new SpaceLocation { Name = "Hurston", Enabled = false, Type = SpaceLocationType.Planet, Children = [] },
                ],
            },
        ];
        var result = await Services.GetRequiredService<MedRunnerAccountContext>().ConfigureAsync(
            new AccountApiTokenCredentials("MedRunner") { SecretToken = "offline-test-token" }, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        Services.GetRequiredService<MedRunnerAccountContext>().ServiceAccessState.CanUseServices.ShouldBeTrue();
    }

    private static async Task SelectLocationAsync(IRenderedComponent<MedRunnerEmergencyCreation> cut, GameStarSystem system,
        GameLocationEntity planetOrMoon, GameLocationEntity? closest = null)
    {
        var selections = cut.FindComponents<GameEntitySelectBox>();
        await cut.InvokeAsync(() => selections[0].Instance.ValueChanged.InvokeAsync(system));
        await cut.InvokeAsync(() => selections[1].Instance.ValueChanged.InvokeAsync(planetOrMoon));
        if (closest is not null)
        {
            await cut.InvokeAsync(() => selections[2].Instance.ValueChanged.InvokeAsync(closest));
        }

        await cut.InvokeAsync(() => cut.FindComponent<MudSelect<ThreatLevel?>>().Instance.ValueChanged.InvokeAsync(ThreatLevel.Low));
    }

    private static Task SubmitAsync(IRenderedComponent<MedRunnerEmergencyCreation> cut)
        => cut.InvokeAsync(() => cut.FindComponents<MudButton>().Single(button => button.Markup.Contains("Submit request", StringComparison.Ordinal))
            .Instance.OnClick.InvokeAsync(new MouseEventArgs()));

    private static Task ContinueDetailsAsync(IRenderedComponent<MedRunnerEmergencyDetails> cut)
        => cut.InvokeAsync(() => cut.FindComponents<MudButton>().Single(button => button.Markup.Contains("Continue", StringComparison.Ordinal))
            .Instance.OnClick.InvokeAsync(new MouseEventArgs()));

    private sealed class RecordingEmergencyEndpoint : IEmergencyEndpoint
    {
        public List<CreateEmergencyRequest> Requests { get; } = [];

        public Task<ApiResponse<Emergency>> CreateEmergencyAsync(CreateEmergencyRequest request)
        {
            Requests.Add(request);
            return Task.FromResult(new ApiResponse<Emergency>(new Emergency
            {
                Id = "offline-emergency", ClientId = "offline-client", ClientRsiHandle = "Pilot", SubscriptionTier = "Test",
                System = request.Location.System, Subsystem = request.Location.Subsystem, TertiaryLocation = request.Location.TertiaryLocation,
                RespondingTeam = new Team { Id = "offline-team", Name = "Test" }, RespondingTeams = [], Status = MissionStatus.Pending,
            }));
        }

        public Task<ApiResponse<Emergency>> GetEmergencyAsync(string emergencyId) => throw new NotSupportedException();
        public Task<ApiResponse<List<Emergency>>> GetEmergenciesAsync(List<string> emergencyIds) => throw new NotSupportedException();
        public Task<ApiResponse<string>> CancelEmergencyWithReasonAsync(string emergencyId, CancellationReason reason) => throw new NotSupportedException();
        public Task<ApiResponse<string>> RateServicesAsync(string emergencyId, ResponseRating rating, string? remarks = null) => throw new NotSupportedException();
        public Task<ApiResponse<TeamDetailsResponse>> TeamDetailsAsync(string emergencyId) => throw new NotSupportedException();
    }

    [Fact]
    public async Task Changing_a_system_clears_dependent_location_selections_and_keeps_alternatives_scoped()
    {
        await ConfigureAccountAsync();
        Services.GetRequiredService<MockOrgSettingsEndpoint>().PublicSettings.LocationSettings.Locations.Add(
            new SpaceLocation { Name = "Pyro", Enabled = true, Children = [new SpaceLocation { Name = "Monox", Enabled = true, Children = [] }] });
        Services.GetRequiredService<MockOrgSettingsEndpoint>().PublicSettings.LocationSettings.Locations[0].Children.Add(
            new SpaceLocation { Name = "microTech", Enabled = true, Children = [] });
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var pyro = new GameStarSystem(2, "Pyro", "PYR");
        var microTech = new GamePlanet(3, "microTech", "MIC", stanton);
        var newBabbage = new GameCity(4, "New Babbage", "NBB", microTech);
        var monox = new GamePlanet(5, "Monox", "MNX", pyro);
        var cut = Render<MedRunnerEmergencyCreation>(parameters => parameters
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
        await ConfigureAccountAsync();
        var settings = Services.GetRequiredService<MockOrgSettingsEndpoint>().PublicSettings.LocationSettings;
        settings.Locations[0].Children.Add(new SpaceLocation { Name = "microTech", Enabled = true, Children = [] });
        settings.Locations[0].Children[1].Enabled = true;
        var stanton = new GameStarSystem(1, "Stanton", "STA");
        var microTech = new GamePlanet(2, "microTech", "MIC", stanton);
        var hurston = new GamePlanet(3, "Hurston", "HUR", stanton);
        var newBabbage = new GameCity(4, "New Babbage", "NBB", microTech);
        var cut = Render<MedRunnerEmergencyCreation>(parameters => parameters
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
