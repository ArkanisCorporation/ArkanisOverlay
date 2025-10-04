namespace Arkanis.Overlay.Components.Services;

using Common.Options;
using Domain.Abstractions.Services;
using LocalLink.Abstractions;
using LocalLink.Models;
using LocalLink.Models.Commands;
using Microsoft.Extensions.Logging;
using Shared;

public class LocalLinkCommandProcessorWithConsent(
    IUserPreferencesManager userPreferencesManager,
    IUserConsentDialogService userConsentDialogService,
    ILogger<LocalLinkCommandProcessorWithConsent> logger
) : ILocalLinkCommandPublisher
{
    public async Task PublishAsync(LocalLinkCommandBase localLinkCommand, CancellationToken cancellationToken)
    {
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

    private async Task SetExternalServiceCredentials(UserPreferences.Credentials credentials)
    {

        var preferences = userPreferencesManager.CurrentPreferences.SetCredentials(credentials);
        await userPreferencesManager.SaveAndApplyUserPreferencesAsync(preferences);
    }
}
