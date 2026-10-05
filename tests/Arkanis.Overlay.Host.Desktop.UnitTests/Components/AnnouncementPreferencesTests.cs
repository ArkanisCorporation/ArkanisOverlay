namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Overlay.Common.Abstractions.Services;
using Overlay.Common.Services;
using Overlay.Components.Services;
using Overlay.Components.Shared;
using Overlay.Components.Shared.Dialogs;
using Overlay.Domain.Abstractions.Services;
using Overlay.Infrastructure.Services;
using Shouldly;

public sealed class AnnouncementPreferencesTests : TestContext
{
    [Fact]
    public async Task SavingAStalePreferencesDialogPreservesNewAnnouncementDismissals()
    {
        var preferences = new InMemoryUserPreferencesManager();
        Services.AddSingleton<IUserPreferencesManager>(preferences);
        Services.AddSingleton<IAppVersionProvider, AssemblyAppVersionProvider>();
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        ComponentFactories.AddStub<UserPreferencesControls>();
        var provider = RenderComponent<MudDialogProvider>();
        await provider.InvokeAsync(() => Services.GetRequiredService<IDialogService>().ShowAsync<UserPreferencesDialog>());
        var controls = provider.FindComponent<Stub<UserPreferencesControls>>();
        controls.Instance.Parameters.Get(x => x.Preferences).BlurBackground = true;
        await provider.InvokeAsync(() => controls.Instance.Parameters.Get(x => x.IsValidChanged).InvokeAsync(true));
        var announcement = new FeatureAnnouncement { Id = "rescue", Title = "Rescue", ContentComponentType = typeof(AnniversaryDialog) };
        var service = new FeatureAnnouncementService(preferences, [announcement], TimeProvider.System);
        await service.ShowPendingAsync(_ => Task.FromResult(true), () => true);

        provider.FindAll("button").Single(x => x.TextContent.Trim() == "Save").Click();

        provider.WaitForAssertion(() => provider.FindComponents<UserPreferencesDialog>().ShouldBeEmpty());
        preferences.CurrentPreferences.BlurBackground.ShouldBeTrue();
        service.Pending.ShouldBeEmpty();
    }
}
