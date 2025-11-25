namespace Arkanis.Overlay.External.Common.Services;

using Microsoft.Extensions.Logging;
using Overlay.Common.Abstractions;
using Overlay.Common.Models;
using Overlay.Common.Services;
using Quartz;

public partial class OidcAuthenticatorRefreshJob<TAuthenticator>(
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
            LogNoValidCredentialsFound(logger, ServiceId);
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
            LogNotExpiringSoon(logger, ServiceId);
            return;
        }

        var result = await authenticator.OidcClient.RefreshTokenAsync(oidcCredentials.RefreshToken);
        if (result.IsError)
        {
            LogRefreshFailed(logger, ServiceId, result.Error, result.ErrorDescription);
            authenticator.RequestRefresh();
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

    [LoggerMessage(LogLevel.Debug, "No OIDC credentials with refresh token found for {serviceId}, skipping refresh")]
    static partial void LogNoValidCredentialsFound(ILogger<OidcAuthenticatorRefreshJob<TAuthenticator>> logger, string serviceId);

    [LoggerMessage(LogLevel.Debug, "OIDC access token for {serviceId} is not expiring soon, skipping refresh")]
    static partial void LogNotExpiringSoon(ILogger<OidcAuthenticatorRefreshJob<TAuthenticator>> logger, string serviceId);

    [LoggerMessage(LogLevel.Error, "Could not refresh OIDC credentials for {serviceId}: {error} - {errorDescription}")]
    static partial void LogRefreshFailed(ILogger<OidcAuthenticatorRefreshJob<TAuthenticator>> logger, string serviceId, string error, string errorDescription);
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
