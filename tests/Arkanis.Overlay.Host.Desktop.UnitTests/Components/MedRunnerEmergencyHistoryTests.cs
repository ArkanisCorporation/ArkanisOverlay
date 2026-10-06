namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Overlay.Common.Abstractions;
using Overlay.Common.Models;
using Overlay.Common.Options;
using Overlay.Components.Shared;
using Overlay.Components.Shared.Dialogs;
using Overlay.External.MedRunner;
using Overlay.External.MedRunner.API;
using Overlay.External.MedRunner.API.Abstractions.Endpoints;
using Overlay.External.MedRunner.API.Endpoints.Emergency.Request;
using Overlay.External.MedRunner.API.Endpoints.Emergency.Response;
using Overlay.External.MedRunner.API.Mocks;
using Overlay.External.MedRunner.API.Mocks.Endpoints;
using Overlay.External.MedRunner.Models;
using Overlay.Infrastructure.Services;
using Overlay.Infrastructure.Services.External;
using Shouldly;

public sealed class MedRunnerEmergencyHistoryTests : BunitContext
{
    public MedRunnerEmergencyHistoryTests()
    {
        Services.AddMudServices();
        Services.AddLogging();
        Services.AddMemoryCache();
        Services.Configure<MedRunnerIntegrationOptions>(options => options.AccountLinkingEnabled = true);
        Services.AddMockMedRunnerApiClient();
        Services.AddSingleton<IUserPreferencesManager, InMemoryUserPreferencesManager>();
        Services.AddSingleton<MedRunnerAccountContext>();
        Services.AddSingleton<PagedClientEndpoint>();
        Services.AddSingleton<IClientEndpoint>(provider => provider.GetRequiredService<PagedClientEndpoint>());
        Services.AddSingleton<ControlledEmergencyEndpoint>();
        Services.AddSingleton<IEmergencyEndpoint>(provider => provider.GetRequiredService<ControlledEmergencyEndpoint>());
        ComponentFactories.AddStub<MedRunnerEmergencyCreation>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Creating_an_alert_opens_details_above_chat_in_the_handling_view()
    {
        await ConfigureAccountAsync();
        var provider = await OpenDialogAsync();
        provider.WaitForAssertion(() => provider.Find(".scenario").ClassList.ShouldNotContain("disabled"));
        await provider.Find(".scenario").ClickAsync(new MouseEventArgs());
        var creation = provider.FindComponent<Stub<MedRunnerEmergencyCreation>>();
        var context = creation.Instance.Parameters.Get(x => x.EmergencyContext);
        context.Emergency = AddEmergency("created-alert", MissionStatus.Pending);

        await provider.InvokeAsync(() => creation.Instance.Parameters.Get(x => x.EmergencyContextChanged).InvokeAsync(context));

        provider.WaitForAssertion(() => provider.FindComponent<MudStepper>().Instance.ActiveStep!.Title.ShouldBe("Handle emergency"));
        provider.FindComponents<MudStep>().Select(step => step.Instance.Title).ShouldNotContain("Provide initial details");
        provider.FindComponent<MedRunnerEmergencyManagement>().FindComponents<MedRunnerEmergencyDetails>().Count.ShouldBe(1);
        provider.Markup.IndexOf("Initial emergency details", StringComparison.Ordinal)
            .ShouldBeLessThan(provider.Markup.IndexOf("Message content", StringComparison.Ordinal));
        provider.Markup.ShouldNotContain("Continue to emergency handling");
    }

    [Fact]
    public async Task Chat_history_and_out_of_order_live_messages_display_oldest_first_without_duplicates()
    {
        Render<MudPopoverProvider>();
        await ConfigureAccountAsync();
        var emergency = AddEmergency("chat-alert", MissionStatus.Accepted);
        var oldest = Chat("oldest", 1);
        var newest = Chat("newest", 3);
        Services.GetRequiredService<MockChatMessageEndpoint>().ChatMessages[emergency.Id] = [newest, oldest];
        var management = Render<MedRunnerEmergencyManagement>(parameters => parameters
            .Add(x => x.EmergencyContext, new MedRunnerComponentBase.EmergencyContextModel { Emergency = emergency }));

        management.WaitForAssertion(() => management.FindAll(".mud-chat-bubble").Select(bubble => bubble.TextContent.Trim())
            .ShouldBe(["oldest", "newest"]));
        var events = Services.GetRequiredService<MockWebSocketEventProvider>();
        await management.InvokeAsync(() => events.SendNewChatMessage(Chat("middle", 2)));
        await management.InvokeAsync(() => events.SendNewChatMessage(Chat("middle", 2)));
        management.WaitForAssertion(() => management.FindAll(".mud-chat-bubble").Select(bubble => bubble.TextContent.Trim())
            .ShouldBe(["oldest", "middle", "newest"]));

        ChatMessage Chat(string id, int minute) => new()
        {
            Id = id, EmergencyId = emergency.Id, SenderId = emergency.ClientId!, Content = id,
            MessageSentTimestamp = new DateTimeOffset(2026, 10, 6, 18, minute, 0, TimeSpan.Zero),
        };
    }

    [Fact]
    public async Task Existing_alerts_load_all_pages_once_and_can_be_reopened_when_new_requests_are_unavailable()
    {
        var active = AddEmergency("active-alert", MissionStatus.Accepted);
        AddEmergency("closed-alert", MissionStatus.Completed);
        AddEmergency("foreign-alert", MissionStatus.Pending, "another-client");
        var client = Services.GetRequiredService<PagedClientEndpoint>();
        client.Pages[""] = Page(["closed-alert", "active-alert"], "next");
        client.Pages["next"] = Page(["active-alert", "foreign-alert"], "next");
        await ConfigureAccountAsync();
        Services.GetRequiredService<MockOrgSettingsEndpoint>().PublicSettings.EmergenciesEnabled = false;
        await Services.GetRequiredService<MedRunnerAccountContext>().RefreshAsync(CancellationToken.None);
        Services.GetRequiredService<MockChatMessageEndpoint>().ChatMessages[active.Id] =
        [new ChatMessage { Id = "chat", EmergencyId = active.Id, SenderId = active.ClientId!, Content = "Waiting at Orison" }];

        var provider = await OpenDialogAsync();

        provider.WaitForAssertion(() => provider.FindAll("[data-emergency-id]").Count.ShouldBe(2));
        provider.FindAll("[data-emergency-id]")[0].GetAttribute("data-emergency-id").ShouldBe("active-alert");
        client.RequestedTokens.Distinct().ShouldBe([null, "next"]);
        provider.WaitForAssertion(() => provider.Find("[data-emergency-id='active-alert']").HasAttribute("disabled").ShouldBeFalse());
        await provider.Find("[data-emergency-id='active-alert']").ClickAsync(new MouseEventArgs());
        provider.FindComponent<MudStepper>().Instance.ActiveIndex.ShouldBe((int)EmergencyDialog.Steps.Handling);
        provider.FindComponent<MudStepper>().Instance.ActiveStep!.Title.ShouldBe("Handle emergency");
        provider.WaitForAssertion(() => provider.FindComponent<MedRunnerEmergencyManagement>().Instance.Emergency!.Id.ShouldBe("active-alert"));
        provider.WaitForAssertion(() => provider.Markup.ShouldContain("Waiting at Orison"), TimeSpan.FromSeconds(5));
        var close = provider.FindComponents<MudButton>().Single(button => button.Find("button").TextContent.Trim() == "Close");
        close.Instance.Disabled.ShouldBeFalse();
        await provider.InvokeAsync(() => close.Instance.OnClick.InvokeAsync(new MouseEventArgs()));
        provider.WaitForAssertion(() => provider.FindComponents<EmergencyDialog>().ShouldBeEmpty());

        await provider.InvokeAsync(() => Services.GetRequiredService<IDialogService>().ShowAsync<EmergencyDialog>());
        provider.WaitForAssertion(() => provider.Find("[data-emergency-id='active-alert']").TextContent.ShouldContain("Return to alert"));
        provider.WaitForAssertion(() => provider.Find("[data-emergency-id='closed-alert']").HasAttribute("disabled").ShouldBeFalse());
        await provider.Find("[data-emergency-id='closed-alert']").ClickAsync(new MouseEventArgs());
        provider.WaitForAssertion(() => provider.FindComponent<MedRunnerEmergencyManagement>().Instance.Emergency!.Id.ShouldBe("closed-alert"));
        provider.FindComponents<MedRunnerEmergencyDetails>().ShouldBeEmpty();
    }

    [Fact]
    public async Task Announced_alerts_and_updates_remain_available_after_the_dialog_is_closed()
    {
        await ConfigureAccountAsync();
        var emergency = AddEmergency("new-alert", MissionStatus.Pending);
        var events = Services.GetRequiredService<MockWebSocketEventProvider>();
        events.SendNewEmergency(emergency);
        var provider = await OpenDialogAsync();
        provider.WaitForAssertion(() => provider.Find("[data-emergency-id='new-alert']").ShouldNotBeNull());

        emergency.Status = MissionStatus.Accepted;
        events.SendEmergencyUpdate(emergency);
        provider.WaitForAssertion(() => provider.Find("[data-emergency-id='new-alert']").HasAttribute("disabled").ShouldBeFalse());
        await provider.Find("[data-emergency-id='new-alert']").ClickAsync(new MouseEventArgs());
        provider.FindComponent<MudStepper>().Instance.ActiveIndex.ShouldBe((int)EmergencyDialog.Steps.Handling);
        provider.FindComponent<MudStepper>().Instance.ActiveStep!.Title.ShouldBe("Handle emergency");
        provider.WaitForAssertion(() => provider.FindComponent<MedRunnerEmergencyManagement>().Instance.Emergency!.Status.ShouldBe(MissionStatus.Accepted));
        var context = Services.GetRequiredService<MedRunnerAccountContext>();
        await provider.InvokeAsync(() => context.ConfigureAsync(
            new AccountApiTokenCredentials("Medrunner") { SecretToken = "offline-test-token" }, CancellationToken.None));
        provider.WaitForAssertion(() => provider.FindComponent<MedRunnerEmergencyManagement>().Instance.Emergency!.Id.ShouldBe("new-alert"));
        events.SendNewEmergency(AddEmergency("unrelated-alert", MissionStatus.Pending, "another-client"));
        provider.FindComponent<MedRunnerEmergencyManagement>().Instance.Emergency!.Id.ShouldBe("new-alert");
        await provider.InvokeAsync(() => context.ConfigureAsync(
            new AccountApiTokenCredentials("Medrunner") { SecretToken = "different-account-token" }, CancellationToken.None));
        provider.WaitForAssertion(() => provider.FindComponents<MedRunnerEmergencyManagement>().ShouldBeEmpty());
    }

    [Fact]
    public async Task History_failures_are_visible_and_can_be_retried_without_losing_loaded_alerts()
    {
        AddEmergency("saved-alert", MissionStatus.Pending);
        var client = Services.GetRequiredService<PagedClientEndpoint>();
        client.Pages[""] = Page(["saved-alert"], null);
        await ConfigureAccountAsync();
        client.FailHistory = true;
        await Services.GetRequiredService<MedRunnerAccountContext>().RefreshAsync(CancellationToken.None);
        var provider = await OpenDialogAsync();
        provider.WaitForAssertion(() => provider.Markup.ShouldContain("history unavailable"));
        provider.Find("[data-emergency-id='saved-alert']").ShouldNotBeNull();

        client.FailHistory = false;
        await provider.InvokeAsync(() => provider.FindComponents<MudButton>().Single(button => button.Markup.Contains("Refresh alerts", StringComparison.Ordinal))
                .Instance.OnClick.InvokeAsync(new MouseEventArgs()));
        provider.WaitForAssertion(() => provider.Markup.ShouldNotContain("history unavailable"));
    }

    private Emergency AddEmergency(string id, MissionStatus status, string? clientId = null)
    {
        var emergency = new Emergency
        {
            Id = id, ClientId = clientId ?? Services.GetRequiredService<MockClientEndpoint>().Person.Id,
            System = "Stanton", Subsystem = "Crusader", ClientRsiHandle = "Pilot", SubscriptionTier = "Test",
            RespondingTeam = new EmergencyResponseTeam { MaxMembers = 6 }, RespondingTeams = [], Status = status,
            CreationTimestamp = 1_791_000_000_000,
            CompletionTimestamp = status is MissionStatus.Completed ? 1_791_000_060_000 : null,
        };
        Services.GetRequiredService<MockEmergencyEndpoint>().Emergencies[id] = emergency;
        return emergency;
    }

    [Fact]
    public async Task Switching_accounts_clears_previous_alerts_even_if_the_new_profile_cannot_be_loaded()
    {
        AddEmergency("old-alert", MissionStatus.Pending);
        var client = Services.GetRequiredService<PagedClientEndpoint>();
        client.Pages[""] = Page(["old-alert"], null);
        await ConfigureAccountAsync();
        var context = Services.GetRequiredService<MedRunnerAccountContext>();
        context.Emergencies.ShouldHaveSingleItem();
        client.FailProfile = true;

        await context.ConfigureAsync(new AccountApiTokenCredentials("Medrunner") { SecretToken = "another-account-token" }, CancellationToken.None);

        context.ClientInfo.ShouldBeNull();
        context.Emergencies.ShouldBeEmpty();
        context.EmergencyLoadError.ShouldNotBeNull();
    }

    [Fact]
    public async Task Live_updates_are_preserved_when_an_older_bulk_response_arrives_afterwards()
    {
        var pending = AddEmergency("active", MissionStatus.Pending);
        Services.GetRequiredService<PagedClientEndpoint>().Pages[""] = Page(["active"], null);
        await ConfigureAccountAsync();
        var context = Services.GetRequiredService<MedRunnerAccountContext>();
        var requested = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var response = new TaskCompletionSource<ApiResponse<List<Emergency>>>(TaskCreationOptions.RunContinuationsAsynchronously);
        Services.GetRequiredService<ControlledEmergencyEndpoint>().GetBulk = _ =>
        {
            requested.TrySetResult();
            return response.Task;
        };
        var refresh = context.RefreshAsync(CancellationToken.None);
        await requested.Task.WaitAsync(TimeSpan.FromSeconds(5), global::Xunit.TestContext.Current.CancellationToken);
        Services.GetRequiredService<MockWebSocketEventProvider>().SendEmergencyUpdate(AddEmergency("active", MissionStatus.Accepted));
        response.SetResult(new ApiResponse<List<Emergency>>([pending]));
        await refresh;

        context.Emergencies.ShouldHaveSingleItem().Status.ShouldBe(MissionStatus.Accepted);
    }

    private ApiPaginatedResponse<ClientHistory> Page(string[] ids, string? token)
        => new()
        {
            Data = ids.Select(id => new ClientHistory
            {
                EmergencyId = id, ClientId = Services.GetRequiredService<MockClientEndpoint>().Person.Id,
                EmergencyCreationTimestamp = DateTimeOffset.FromUnixTimeSeconds(1_791_000_000),
            }).ToList(),
            PaginationToken = token,
        };

    private async Task ConfigureAccountAsync()
    {
        var context = Services.GetRequiredService<MedRunnerAccountContext>();
        await context.InitializeAsync(CancellationToken.None);
        var result = await context.ConfigureAsync(new AccountApiTokenCredentials("Medrunner") { SecretToken = "offline-test-token" }, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    private async Task<IRenderedComponent<MudDialogProvider>> OpenDialogAsync()
    {
        Render<MudPopoverProvider>();
        var provider = Render<MudDialogProvider>();
        await provider.InvokeAsync(() => Services.GetRequiredService<IDialogService>().ShowAsync<EmergencyDialog>());
        provider.WaitForAssertion(() => provider.FindComponent<EmergencyDialog>().ShouldNotBeNull());
        return provider;
    }

    private sealed class PagedClientEndpoint(MockClientEndpoint inner) : IClientEndpoint
    {
        public Dictionary<string, ApiPaginatedResponse<ClientHistory>> Pages { get; } = new()
        {
            [""] = new() { Data = [] },
        };
        public List<string?> RequestedTokens { get; } = [];
        public bool FailHistory { get; set; }
        public bool FailProfile { get; set; }
        public Task<ApiResponse<ApiPaginatedResponse<ClientHistory>>> GetHistoryAsync(int limit, string? paginationToken = null)
        {
            RequestedTokens.Add(paginationToken);
            return Task.FromResult(FailHistory
                ? new ApiResponse<ApiPaginatedResponse<ClientHistory>> { ErrorMessage = "history unavailable" }
                : new ApiResponse<ApiPaginatedResponse<ClientHistory>>(Pages[paginationToken ?? ""]));
        }
        public Task<ApiResponse<Person>> GetAsync() => FailProfile
            ? Task.FromResult(new ApiResponse<Person> { ErrorMessage = "profile unavailable" })
            : inner.GetAsync();
        public Task<ApiResponse<ClientBlockedStatus>> GetBlockedStatusAsync() => inner.GetBlockedStatusAsync();
        public Task<ApiResponse<Person>> LinkClientAsync(string rsiHandle) => inner.LinkClientAsync(rsiHandle);
        public Task<ApiResponse<string>> SetUserSettingsAsync(string settings) => inner.SetUserSettingsAsync(settings);
        public Task<ApiResponse<string>> DeactivateAsync() => inner.DeactivateAsync();
    }

    private sealed class ControlledEmergencyEndpoint(MockEmergencyEndpoint inner) : IEmergencyEndpoint
    {
        public Func<List<string>, Task<ApiResponse<List<Emergency>>>>? GetBulk { get; set; }
        public Task<ApiResponse<List<Emergency>>> GetEmergenciesAsync(List<string> emergencyIds)
            => GetBulk?.Invoke(emergencyIds) ?? inner.GetEmergenciesAsync(emergencyIds);
        public Task<ApiResponse<Emergency>> GetEmergencyAsync(string emergencyId) => inner.GetEmergencyAsync(emergencyId);
        public Task<ApiResponse<Emergency>> CreateEmergencyAsync(CreateEmergencyRequest request) => inner.CreateEmergencyAsync(request);
        public Task<ApiResponse<string>> CancelEmergencyWithReasonAsync(string emergencyId, CancellationReason reason) => inner.CancelEmergencyWithReasonAsync(emergencyId, reason);
        public Task<ApiResponse<string>> RateServicesAsync(string emergencyId, ResponseRating rating, string? remarks = null) => inner.RateServicesAsync(emergencyId, rating, remarks);
        public Task<ApiResponse<TeamDetailsResponse>> TeamDetailsAsync(string emergencyId) => inner.TeamDetailsAsync(emergencyId);
    }
}
