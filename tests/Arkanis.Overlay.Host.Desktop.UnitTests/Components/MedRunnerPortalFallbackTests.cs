namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using global::Arkanis.Overlay.Common.Abstractions;

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
using Overlay.Infrastructure.Options;
using Overlay.Common.Options;
using Shouldly;

public sealed class MedRunnerPortalFallbackTests : BunitContext
{
    public MedRunnerPortalFallbackTests()
    {
        Services.AddMudServices();
        Services.AddLogging();
        Services.AddMemoryCache();
        Services.Configure<MedRunnerIntegrationOptions>(options => options.AccountLinkingEnabled = false);
        Services.AddOptions<InfrastructureServiceOptions>();
        Services.AddMockMedRunnerApiClient();
        Services.AddSingleton<IUserPreferencesManager, InMemoryUserPreferencesManager>();
        Services.AddSingleton<MedRunnerAccountContext>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task EmergencyViewOffersThePortalAndProviderContactWithoutInAppRequestSteps()
    {
        var provider = Render<MudDialogProvider>();
        var service = Services.GetRequiredService<IDialogService>();
        await provider.InvokeAsync(() => service.ShowAsync<EmergencyDialog>());
        provider.WaitForAssertion(() => provider.FindComponent<EmergencyDialog>().ShouldNotBeNull());

        var portal = provider.Find("a[href^='https://portal.medrunner.space/emergency']");
        portal.TextContent.ShouldContain("Search & Rescue");
        portal.GetAttribute("target").ShouldBe("_blank");
        var contact = provider.Find("a[href^='mailto:business+overlay@Arkanis.cc']");
        contact.GetAttribute("href")!.ShouldContain("Service%20Provider%20Feature%20Inquiry");
        provider.FindComponents<MedRunnerEmergencyCreation>().ShouldBeEmpty();
        provider.FindComponents<MedRunnerEmergencyDetails>().ShouldBeEmpty();
        provider.FindComponents<MudStep>().Count.ShouldBe(1);
    }

    [Fact]
    public async Task SetupDialogDoesNotOfferTokenEntryWhileLinkingIsDisabled()
    {
        var provider = Render<MudDialogProvider>();
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
        ComponentFactories.AddStub<CitizenIdServiceSettingsPanel>();
        var controls = Render<ExternalAccountControls>(parameters => parameters.Add(x => x.ContentId, "test"));

        controls.Markup.ShouldContain("Account linking temporarily unavailable");
        controls.FindComponents<MedRunnerAccountInfo>().ShouldBeEmpty();
        controls.FindComponents<MudButton>().ShouldBeEmpty();
    }
}
