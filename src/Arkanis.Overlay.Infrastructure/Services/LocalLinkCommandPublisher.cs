namespace Arkanis.Overlay.Infrastructure.Services;

using Common.Options;
using Domain.Abstractions.Services;
using LocalLink.Abstractions;
using LocalLink.Models;
using LocalLink.Models.Commands;
using Microsoft.Extensions.Logging;

public class LocalLinkCommandProcessor(IUserPreferencesManager userPreferencesManager, ILogger<LocalLinkCommandProcessor> logger) : ILocalLinkCommandPublisher
{
    public Task PublishAsync(LocalLinkCommandBase localLinkCommand, CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing command: {@Command}", localLinkCommand);
        return localLinkCommand switch
        {
            SetExternalServiceCredentialsCommand data => SetExternalServiceCredentials(data.Credentials),
            _ => Task.CompletedTask,
        };
    }

    private async Task SetExternalServiceCredentials(UserPreferences.Credentials credentials)
    {
        var preferences = userPreferencesManager.CurrentPreferences.SetCredentials(credentials);
        await userPreferencesManager.SaveAndApplyUserPreferencesAsync(preferences);
    }
}
