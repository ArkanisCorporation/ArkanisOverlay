namespace Arkanis.Overlay.External.Common.Services;

using Microsoft.Extensions.Logging;
using Overlay.Common.Abstractions;
using Overlay.Common.Models;
using Overlay.Common.Services;
using Quartz;

public class OidcAuthenticatorRefreshJob<TAuthenticator>(
    TAuthenticator authenticator,
    IUserPreferencesManager preferencesManager,
    ILogger<OidcAuthenticatorRefreshJob<TAuthenticator>> logger
) : IJob where TAuthenticator : OidcAuthenticator
{
    private string ServiceId
        => authenticator.AuthenticatorInfo.ServiceId;

    public async Task Execute(IJobExecutionContext context)
    {
        var credentials = preferencesManager.CurrentPreferences.GetCredentialsOrDefaultFor(ServiceId);
        if (credentials is not AccountOidcCredentials { RefreshToken.Length: > 0 } oidcCredentials)
        {
            logger.LogDebug("No OIDC credentials with refresh token found for {ServiceId}, skipping refresh", ServiceId);
            return;
        }

        if (!context.MergedJobDataMap.TryGetTimeSpan(OidcAuthenticatorRefreshJob.RefreshThresholdKey, out var refreshThreshold))
        {
            refreshThreshold = TimeSpan.FromMinutes(30);
        }

        var minimumValidDate = DateTimeOffset.UtcNow.Add(refreshThreshold);
        if (oidcCredentials.AccessTokenExpiresAt > minimumValidDate
            || (oidcCredentials.ReadAccessTokenAsJwt() is { } jwt && jwt.ValidTo > minimumValidDate))
        {
            logger.LogDebug("OIDC access token for {ServiceId} is not expiring soon, skipping refresh", ServiceId);
            return;
        }

        var result = await authenticator.OidcClient.RefreshTokenAsync(oidcCredentials.RefreshToken);
        if (result.IsError)
        {
            logger.LogError(
                "Could not refresh OIDC credentials for {ServiceId}: {Error} - {ErrorDescription}",
                ServiceId,
                result.Error,
                result.ErrorDescription
            );
            return;
        }

        var updatedCredentials = oidcCredentials with
        {
            AccessToken = result.AccessToken,
            AccessTokenExpiresAt = result.AccessTokenExpiration,
            RefreshToken = result.RefreshToken,
            IdToken = result.IdentityToken,
        };
        var updatedPreferences = preferencesManager.CurrentPreferences.SetCredentials(updatedCredentials);
        await preferencesManager.SaveAndApplyUserPreferencesAsync(updatedPreferences);
    }
}

public static class OidcAuthenticatorRefreshJob
{
    public const string RefreshThresholdKey = nameof(RefreshThresholdKey);

    public static JobDataMap CreateJobData(TimeSpan? refreshThreshold = null)
    {
        var dataMap = new JobDataMap();
        if (refreshThreshold.HasValue)
        {
            dataMap.Put(RefreshThresholdKey, refreshThreshold.Value);
        }

        return dataMap;
    }
}
