namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Overlay.Components.Shared;
using Overlay.Components.Shared.Dialogs;
using Overlay.Domain.Abstractions.Services;
using Overlay.External.MedRunner;
using Overlay.Infrastructure.Services;
using Overlay.Infrastructure.Services.External;
using Shouldly;

public sealed class MedRunnerPortalFallbackTests : TestContext
{
    public MedRunnerPortalFallbackTests()
    {
        Services.AddMudServices();
        Services.AddLogging();
        Services.AddMockMedRunnerApiClient();
        Services.AddSingleton<IUserPreferencesManager, InMemoryUserPreferencesManager>();
        Services.AddSingleton<MedRunnerAccountContext>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task EmergencyViewOffersThePortalAndProviderContactWithoutInAppRequestSteps()
    {
        var provider = RenderComponent<MudDialogProvider>();
        var service = Services.GetRequiredService<IDialogService>();
        await provider.InvokeAsync(() => service.ShowAsync<EmergencyDialog>());
        provider.WaitForAssertion(() => provider.FindComponent<EmergencyDialog>().ShouldNotBeNull());

        var portal = provider.Find("a[href^='https://portal.medrunner.space/emergency']");
        portal.TextContent.ShouldContain("Search & Rescue");
        portal.GetAttribute("target").ShouldBe("_blank");
        var contact = provider.Find("a[href='https://join.arkanis.cc/']");
        contact.TextContent.ShouldContain("Contact us");
        contact.GetAttribute("target").ShouldBe("_blank");
        provider.FindComponents<MedRunnerEmergencyCreation>().ShouldBeEmpty();
        provider.FindComponents<MedRunnerEmergencyDetails>().ShouldBeEmpty();
        provider.FindComponents<MudStep>().Count.ShouldBe(1);
    }

    [Fact]
    public async Task SetupDialogDoesNotOfferTokenEntryWhileLinkingIsDisabled()
    {
        var provider = RenderComponent<MudDialogProvider>();
        await provider.InvokeAsync(() => Services.GetRequiredService<IDialogService>().ShowAsync<MedRunnerSetupDialog>());
        provider.WaitForAssertion(() => provider.Markup.ShouldContain("temporarily unavailable"));

        provider.FindAll("input").ShouldBeEmpty();
        provider.FindComponents<MudStepper>().ShouldBeEmpty();
        provider.Find("a[href^='https://portal.medrunner.space/']").GetAttribute("target").ShouldBe("_blank");
    }

    [Fact]
    public void SettingsDoNotOfferMedRunnerLinkOrUnlinkControlsWhileDisabled()
    {
        ComponentFactories.AddStub<UexServiceSettingsPanel>();
        var controls = RenderComponent<ExternalAccountControls>(parameters => parameters.Add(x => x.ContentId, "test"));

        controls.Markup.ShouldContain("Account linking temporarily unavailable");
        controls.FindComponents<MedRunnerAccountInfo>().ShouldBeEmpty();
        controls.FindComponents<MudButton>().ShouldBeEmpty();
    }
}
