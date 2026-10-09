namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using System.Security.Claims;
using Bunit;
using global::CitizenId.Domain.Shared.Authorization;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;
using Overlay.Common;
using Overlay.Common.Abstractions;
using Overlay.Common.Enums;
using Overlay.Common.Models;
using Overlay.Common.Options;
using Overlay.Common.Services;
using Overlay.Components.Shared;
using Overlay.Components.Shared.External;
using Overlay.External.CitizenId;
using Overlay.External.CitizenId.Options;
using Overlay.External.MedRunner;
using Overlay.Infrastructure.Options;
using Overlay.Infrastructure.Services;
using Overlay.Infrastructure.Services.External;
using Shouldly;

public sealed class ExternalAccountControlsTests : BunitContext
{
    public ExternalAccountControlsTests()
    {
        Services.AddMudServices();
        Services.AddLogging();
        Services.AddMemoryCache();
        Services.AddOptions<CitizenIdOptions>();
        Services.AddOptions<ArkanisRestBackendOptions>();
        Services.AddSingleton<IUserPreferencesManager, InMemoryUserPreferencesManager>();
        Services.AddSingleton<TestCitizenIdAuthenticator>();
        Services.AddSingleton<CitizenIdAuthenticator>(provider => provider.GetRequiredService<TestCitizenIdAuthenticator>());
        Services.AddSingleton<CitizenIdLinkHelper>();
        Services.AddSingleton<CitizenIdAccountContext>();
        Services.AddMockMedRunnerApiClient();
        Services.AddSingleton<MedRunnerAccountContext>();
        ComponentFactories.AddStub<UexServiceSettingsPanel>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(HostingMode.LocalSingleUser, false)]
    [InlineData(HostingMode.LocalSingleUser, true)]
    [InlineData(HostingMode.Server, false)]
    [InlineData(HostingMode.Server, true)]
    public void CitizenIdArkanisAndRsiSettingsCoexistWithMedRunner(HostingMode hostingMode, bool medRunnerEnabled)
    {
        Services.Configure<InfrastructureServiceOptions>(options => options.HostingMode = hostingMode);
        Services.Configure<MedRunnerIntegrationOptions>(options => options.AccountLinkingEnabled = medRunnerEnabled);

        var controls = RenderControls("account-settings");

        controls.FindComponent<CitizenIdServiceSettingsPanel>().ShouldNotBeNull();
        controls.FindComponent<CitizenIdLogo>().ShouldNotBeNull();
        controls.FindComponent<RsiLogo>().ShouldNotBeNull();
        controls.FindComponent<ArkanisCorpLogo>().ShouldNotBeNull();
        controls.FindComponent<MedRunnerLogo>().ShouldNotBeNull();
        controls.Find("a[href$='/api/v1/overlay/connect/link-account/overlay']").TextContent.ShouldContain("Connect account");
        controls.Markup.Contains("Do not link your personal accounts!", StringComparison.Ordinal)
            .ShouldBe(hostingMode is HostingMode.Server);
        controls.Markup.Contains("Account linking temporarily unavailable", StringComparison.Ordinal)
            .ShouldBe(!medRunnerEnabled);
        controls.FindComponents<MedRunnerAccountInfo>().Count.ShouldBe(medRunnerEnabled ? 1 : 0);
    }

    [Fact]
    public async Task LinkedCitizenIdStillDisplaysItsIdentityAndVerifiedRsiAccount()
    {
        var authenticator = Services.GetRequiredService<TestCitizenIdAuthenticator>();
        authenticator.NextIdentity = CreateIdentity("Citizen user", "TestPilot");
        var context = Services.GetRequiredService<CitizenIdAccountContext>();
        await context.ConfigureAsync(new AccountOidcCredentials(ExternalService.CitizenId) { AccessToken = "test-token" }, CancellationToken.None);

        var controls = RenderControls("linked-account");

        controls.Markup.ShouldContain("Citizen user");
        controls.Find("a[href='https://robertsspaceindustries.com/citizens/TestPilot']").TextContent.ShouldContain("TestPilot");
        controls.Find("a[href$='/api/v1/overlay/connect/link-account/overlay']").TextContent.ShouldContain("Connect different account");
    }

    [Fact]
    public async Task CitizenIdRefreshRequestsUpdateTheDerivedRsiIdentity()
    {
        var authenticator = Services.GetRequiredService<TestCitizenIdAuthenticator>();
        authenticator.NextIdentity = CreateIdentity("Citizen user", "OriginalPilot");
        var context = Services.GetRequiredService<CitizenIdAccountContext>();
        await context.InitializeAsync(CancellationToken.None);
        await context.ConfigureAsync(new AccountOidcCredentials(ExternalService.CitizenId) { AccessToken = "test-token" }, CancellationToken.None);
        context.RsiIdentity.Name.ShouldBe("OriginalPilot");

        authenticator.NextIdentity = CreateIdentity("Citizen user", "UpdatedPilot");
        authenticator.RequestRefresh();

        context.RsiIdentity.Name.ShouldBe("UpdatedPilot");
        context.IsAuthenticated.ShouldBeTrue();
        await context.UnlinkAsync(CancellationToken.None);
        context.IsAuthenticated.ShouldBeFalse();
        context.RsiIdentity.IsAuthenticated.ShouldBeFalse();
    }

    private static ClaimsIdentity CreateIdentity(string displayName, string rsiUsername)
        => new([
            new Claim(AccountClaimTypes.DisplayName, displayName),
            new Claim(CitizenIdClaims.User.Rsi.Username, rsiUsername),
        ], ExternalService.CitizenId, AccountClaimTypes.DisplayName, null);

    private IRenderedComponent<ExternalAccountControls> RenderControls(string contentId)
    {
        Render<MudPopoverProvider>();
        return Render<ExternalAccountControls>(parameters => parameters.Add(x => x.ContentId, contentId));
    }

    private sealed class TestCitizenIdAuthenticator(IServiceProvider provider, IOptionsMonitor<CitizenIdOptions> options)
        : CitizenIdAuthenticator(provider, options)
    {
        public ClaimsIdentity NextIdentity { get; set; } = new();

        public override AuthenticationTask AuthenticateAsync(AccountCredentials credentials, CancellationToken cancellationToken)
            => new StaticAuthenticationTask(this, credentials, NextIdentity, cancellationToken);
    }

    private sealed class StaticAuthenticationTask(
        CitizenIdAuthenticator authenticator,
        AccountCredentials credentials,
        ClaimsIdentity identity,
        CancellationToken cancellationToken
    ) : OidcAuthenticator.AuthenticationTask(authenticator, credentials, NullLogger<OidcAuthenticator.AuthenticationTask>.Instance, cancellationToken)
    {
        protected override Task<Result<ClaimsIdentity>> RunAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Identity = identity;
            return Task.FromResult(Result.Ok(Identity));
        }
    }
}
