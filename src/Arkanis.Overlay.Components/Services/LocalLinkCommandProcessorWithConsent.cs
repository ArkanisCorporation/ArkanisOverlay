namespace Arkanis.Overlay.Components.Services;

using Common.Abstractions;
using Common;
using Common.Models;
using Common.Options;
using Domain.Abstractions.Services;
using LocalLink.Abstractions;
using LocalLink.Models;
using LocalLink.Models.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;

public class LocalLinkCommandProcessorWithConsent(
    IUserPreferencesManager userPreferencesManager,
    IUserConsentDialogService userConsentDialogService,
    ILogger<LocalLinkCommandProcessorWithConsent> logger,
    IOptions<MedRunnerIntegrationOptions>? integrationOptions = null
) : ILocalLinkCommandPublisher
{
    public async Task PublishAsync(LocalLinkCommandBase localLinkCommand, CancellationToken cancellationToken)
    {
        if (localLinkCommand is SetExternalServiceCredentialsCommand { Credentials.ServiceId: ExternalService.Medrunner }
            && integrationOptions?.Value.AccountLinkingEnabled is not true)
        {
            logger.LogInformation("Ignoring Medrunner account linking while the integration is disabled");
            return;
        }

        logger.LogDebug("Processing command: {@Command}", localLinkCommand);

        var consentParameters = LocalLinkCommandUserConsent.GetParameters(localLinkCommand);
        var result = await userConsentDialogService.RequestConsentAsync<LocalLinkCommandUserConsent>(consentParameters);
        if (!result.WasAccepted)
        {
            return;
        }

        var actionTask = localLinkCommand switch
        {
            SetExternalServiceCredentialsCommand data => SetExternalServiceCredentials(data.Credentials),
            _ => Task.CompletedTask,
        };
        await actionTask;
    }

    private async Task SetExternalServiceCredentials(AccountCredentials credentials)
    {
        var preferences = userPreferencesManager.CurrentPreferences.SetCredentials(credentials);
        await userPreferencesManager.SaveAndApplyUserPreferencesAsync(preferences);
    }
}
