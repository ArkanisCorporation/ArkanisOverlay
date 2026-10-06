namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using Common;
using Common.Models;
using Common.Options;
using Domain.Abstractions.Services;
using Infrastructure.Services;
using LocalLink.Models.Commands;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Overlay.Components.Services;
using Shouldly;

public sealed class LocalLinkMedRunnerGateTests
{
    [Fact]
    public async Task DisabledMedRunnerImportsDoNotRequestConsentOrReplaceSavedCredentials()
    {
        var preferences = new InMemoryUserPreferencesManager();
        var saved = new AccountApiTokenCredentials(ExternalService.Medrunner) { SecretToken = "saved-token" };
        await preferences.SaveAndApplyUserPreferencesAsync(preferences.CurrentPreferences.SetCredentials(saved));
        var consent = new ConsentingDialog();
        var processor = new LocalLinkCommandProcessorWithConsent(preferences, consent, NullLogger<LocalLinkCommandProcessorWithConsent>.Instance);

        await processor.PublishAsync(
            new SetExternalServiceCredentialsCommand
            {
                Credentials = new AccountApiTokenCredentials(ExternalService.Medrunner) { SecretToken = "incoming-token" },
            },
            CancellationToken.None
        );

        consent.Requests.ShouldBe(0);
        preferences.CurrentPreferences.ExternalServiceCredentials.ShouldBe([saved]);
    }

    [Theory]
    [InlineData(ExternalService.UnitedExpress, false)]
    [InlineData(ExternalService.Medrunner, true)]
    public async Task OtherServicesAndExplicitlyEnabledMedRunnerStillRequireConsent(string serviceId, bool medRunnerEnabled)
    {
        var preferences = new InMemoryUserPreferencesManager();
        var consent = new ConsentingDialog();
        var processor = new LocalLinkCommandProcessorWithConsent(
            preferences,
            consent,
            NullLogger<LocalLinkCommandProcessorWithConsent>.Instance,
            Options.Create(new MedRunnerIntegrationOptions { AccountLinkingEnabled = medRunnerEnabled })
        );
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
