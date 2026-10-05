namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Overlay.Common;
using Overlay.Common.Models;
using Overlay.Common.Options;
using Overlay.Components.Services;
using Overlay.Domain.Abstractions.Services;
using Overlay.Infrastructure.Services;
using Overlay.LocalLink.Models.Commands;
using Shouldly;

public sealed class LocalLinkMedRunnerGateTests
{
    [Fact]
    public async Task DisabledMedRunnerImportsDoNotRequestConsentOrReplaceSavedCredentials()
    {
        var preferences = new InMemoryUserPreferencesManager();
        var saved = new AccountApiTokenCredentials(ExternalService.MedRunner) { SecretToken = "saved-token" };
        await preferences.SaveAndApplyUserPreferencesAsync(preferences.CurrentPreferences.SetCredentials(saved));
        var consent = new ConsentingDialog();
        var processor = new LocalLinkCommandProcessorWithConsent(preferences, consent, NullLogger<LocalLinkCommandProcessorWithConsent>.Instance);

        await processor.PublishAsync(new SetExternalServiceCredentialsCommand
        {
            Credentials = new AccountApiTokenCredentials(ExternalService.MedRunner) { SecretToken = "incoming-token" },
        }, CancellationToken.None);

        consent.Requests.ShouldBe(0);
        preferences.CurrentPreferences.ExternalServiceCredentials.ShouldBe([saved]);
    }

    [Theory]
    [InlineData(ExternalService.UnitedExpress, false)]
    [InlineData(ExternalService.MedRunner, true)]
    public async Task OtherServicesAndExplicitlyEnabledMedRunnerStillRequireConsent(string serviceId, bool medRunnerEnabled)
    {
        var preferences = new InMemoryUserPreferencesManager();
        var consent = new ConsentingDialog();
        var processor = new LocalLinkCommandProcessorWithConsent(preferences, consent,
            NullLogger<LocalLinkCommandProcessorWithConsent>.Instance,
            Options.Create(new MedRunnerIntegrationOptions { AccountLinkingEnabled = medRunnerEnabled }));
        var credentials = new AccountApiTokenCredentials(serviceId) { SecretToken = "incoming-token" };

        await processor.PublishAsync(new SetExternalServiceCredentialsCommand { Credentials = credentials }, CancellationToken.None);

        consent.Requests.ShouldBe(1);
        preferences.CurrentPreferences.GetCredentialsOrDefaultFor(serviceId).ShouldBe(credentials);
    }

    private sealed class ConsentingDialog : IUserConsentDialogService
    {
        public int Requests { get; private set; }

        public Task<IUserConsentDialogService.Result> RequestConsentAsync<T>(IDictionary<string, object> parameters)
            where T : ComponentBase, IUserConsentDialogService.IContent
        {
            Requests++;
            return Task.FromResult(IUserConsentDialogService.Result.Consent);
        }
    }
}
