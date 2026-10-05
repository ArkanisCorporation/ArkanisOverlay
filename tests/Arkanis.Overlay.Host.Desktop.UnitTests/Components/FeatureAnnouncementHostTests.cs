namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using global::Arkanis.Overlay.Common.Abstractions;

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Overlay.Components.Services;
using Overlay.Components.Shared.Announcements;
using Overlay.Components.Shared.Dialogs;
using Overlay.Domain.Abstractions.Services;
using Overlay.Infrastructure.Services;
using Shouldly;

public sealed class FeatureAnnouncementHostTests : BunitContext
{
    private readonly InMemoryUserPreferencesManager _preferences = new();

    public FeatureAnnouncementHostTests()
    {
        Services.AddMudServices();
        Services.AddLogging();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IUserPreferencesManager>(_preferences);
        Services.AddSingleton(TimeProvider.System);
        Services.AddSingleton(new FeatureAnnouncement
        {
            Id = "first", Title = "First feature", ContentComponentType = typeof(EmergencyServicesAnnouncement), Priority = 10,
        });
        Services.AddSingleton(new FeatureAnnouncement
        {
            Id = "second", Title = "Second feature", ContentComponentType = typeof(EmergencyServicesAnnouncement),
        });
        Services.AddScoped<FeatureAnnouncementService>();
    }

    [Fact]
    public void ExplicitDismissalAdvancesTheQueueAndDisablesBackdropAndEscapeDismissal()
    {
        var provider = Render<MudDialogProvider>();
        Render<FeatureAnnouncementHost>();
        provider.WaitForAssertion(() => provider.FindComponent<FeatureAnnouncementDialog>().Instance.Announcement.Id.ShouldBe("first"));
        var first = provider.FindComponent<FeatureAnnouncementDialog>();
        first.Instance.MudDialog.Options.BackdropClick.ShouldBe(false);
        first.Instance.MudDialog.Options.CloseOnEscapeKey.ShouldBe(false);
        first.Instance.MudDialog.Options.CloseButton.ShouldBe(false);

        provider.Find("button").Click();
        provider.WaitForAssertion(() => provider.FindComponent<FeatureAnnouncementDialog>().Instance.Announcement.Id.ShouldBe("second"));
        provider.FindComponents<FeatureAnnouncementDialog>().Count.ShouldBe(1);
        _preferences.CurrentPreferences.DismissedFeatureAnnouncements.ShouldContain("first:1");

        provider.Find("button").Click();
        provider.WaitForAssertion(() => provider.FindComponents<FeatureAnnouncementDialog>().ShouldBeEmpty());
        Services.GetRequiredService<FeatureAnnouncementService>().Pending.ShouldBeEmpty();
    }

    [Fact]
    public async Task InteractiveDialogInterruptsTheAnnouncementAndResumesItWithoutDismissingIt()
    {
        var provider = Render<MudDialogProvider>();
        Render<FeatureAnnouncementHost>();
        provider.WaitForAssertion(() => provider.FindComponents<FeatureAnnouncementDialog>().Count.ShouldBe(1));
        var service = Services.GetRequiredService<IDialogService>();

        await provider.InvokeAsync(() => service.ShowAsync<TestDialog>("Emergency"));
        provider.WaitForAssertion(() => provider.FindComponents<FeatureAnnouncementDialog>().ShouldBeEmpty());
        _preferences.CurrentPreferences.DismissedFeatureAnnouncements.ShouldBeEmpty();

        provider.FindComponent<TestDialog>().Find("button").Click();
        provider.WaitForAssertion(() => provider.FindComponent<FeatureAnnouncementDialog>().Instance.Announcement.Id.ShouldBe("first"));
    }

    [Fact]
    public async Task DisposingTheHostDoesNotDismissTheVisibleAnnouncement()
    {
        var provider = Render<MudDialogProvider>();
        var host = Render<FeatureAnnouncementHost>();
        provider.WaitForAssertion(() => provider.FindComponents<FeatureAnnouncementDialog>().Count.ShouldBe(1));

        await provider.InvokeAsync(host.Instance.Dispose);

        _preferences.CurrentPreferences.DismissedFeatureAnnouncements.ShouldBeEmpty();
        provider.WaitForAssertion(() => provider.FindComponents<FeatureAnnouncementDialog>().ShouldBeEmpty());
    }

    [Fact]
    public async Task BackgroundDialogRequestInterruptsTheAnnouncementOnTheRendererDispatcher()
    {
        var provider = Render<MudDialogProvider>();
        Render<FeatureAnnouncementHost>();
        provider.WaitForAssertion(() => provider.FindComponents<FeatureAnnouncementDialog>().Count.ShouldBe(1));
        var service = Services.GetRequiredService<IDialogService>();

        await Task.Run(() => service.ShowAsync<TestDialog>("Consent"));

        provider.WaitForAssertion(() => provider.FindComponents<FeatureAnnouncementDialog>().ShouldBeEmpty());
        provider.FindComponent<TestDialog>().Find("button").Click();
        provider.WaitForAssertion(() => provider.FindComponent<FeatureAnnouncementDialog>().Instance.Announcement.Id.ShouldBe("first"));
    }

    [Fact]
    public async Task ProviderTeardownReleasesTheQueueForTheNextLayout()
    {
        var provider = Render<MudDialogProvider>();
        Render<FeatureAnnouncementHost>();
        provider.WaitForAssertion(() => provider.FindComponents<FeatureAnnouncementDialog>().Count.ShouldBe(1));

        await DisposeComponentsAsync();

        var nextProvider = Render<MudDialogProvider>();
        Render<FeatureAnnouncementHost>();
        nextProvider.WaitForAssertion(() => nextProvider.FindComponent<FeatureAnnouncementDialog>().Instance.Announcement.Id.ShouldBe("first"));
        _preferences.CurrentPreferences.DismissedFeatureAnnouncements.ShouldBeEmpty();
    }

    [Fact]
    public void AnotherDialogOpenedDuringAnnouncementRenderingTakesPrecedence()
    {
        var provider = Render<MudDialogProvider>();
        var service = Services.GetRequiredService<IDialogService>();
        var openedOther = false;
        service.DialogInstanceAddedAsync += async _ =>
        {
            if (openedOther) return;
            openedOther = true;
            await service.ShowAsync<TestDialog>("Emergency during startup");
        };

        Render<FeatureAnnouncementHost>();

        provider.WaitForAssertion(() =>
        {
            provider.FindComponents<TestDialog>().Count.ShouldBe(1);
            provider.FindComponents<FeatureAnnouncementDialog>().ShouldBeEmpty();
        });
        provider.FindComponent<TestDialog>().Find("button").Click();
        provider.WaitForAssertion(() => provider.FindComponent<FeatureAnnouncementDialog>().Instance.Announcement.Id.ShouldBe("first"));
    }

    [Fact]
    public async Task TeardownWhileOpeningReleasesTheQueueForAnImmediatelyMountedReplacement()
    {
        var provider = Render<MudDialogProvider>();
        var service = Services.GetRequiredService<IDialogService>();
        var opened = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task DelayOpeningAsync(IDialogReference _)
        {
            opened.TrySetResult();
            await release.Task;
        }
        Render<DelayedAnnouncementHost>(parameters => parameters.Add(x => x.Delay, DelayOpeningAsync));
        await opened.Task;

        await DisposeComponentsAsync();
        service.DialogInstanceAddedAsync -= DelayOpeningAsync;
        var nextProvider = Render<MudDialogProvider>();
        Render<FeatureAnnouncementHost>();
        release.TrySetResult();

        nextProvider.WaitForAssertion(() => nextProvider.FindComponent<FeatureAnnouncementDialog>().Instance.Announcement.Id.ShouldBe("first"));
        _preferences.CurrentPreferences.DismissedFeatureAnnouncements.ShouldBeEmpty();
    }

    public sealed class DelayedAnnouncementHost : FeatureAnnouncementHost
    {
        [Inject]
        public required IDialogService Service { get; set; }

        [Parameter]
        public required Func<IDialogReference, Task> Delay { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            // The service awaits the last multicast event handler's task.
            Service.DialogInstanceAddedAsync += Delay;
        }
    }

    public sealed class TestDialog : ComponentBase
    {
        [CascadingParameter]
        public required IMudDialogInstance MudDialog { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<MudDialog>(0);
            builder.AddAttribute(1, "DialogContent", (RenderFragment)(content =>
            {
                content.OpenElement(0, "button");
                content.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, () => MudDialog.Cancel()));
                content.AddContent(2, "Close emergency");
                content.CloseElement();
            }));
            builder.CloseComponent();
        }
    }
}
