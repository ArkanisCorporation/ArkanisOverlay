namespace Arkanis.Overlay.Infrastructure.Helpers;

using Common;
using External.UEX;
using Microsoft.AspNetCore.Http;

public static class ExternalLinkHelper
{
    private static string AddAttributionGoogleAnalyticsTo(string url, string? contentId = null, Dictionary<string, string>? queryParams = null)
    {
        queryParams = new Dictionary<string, string>(queryParams ?? [])
        {
            ["utm_source"] = "Arkanis",
            ["utm_medium"] = "referral",
            ["utm_campaign"] = "Attribution",
        };
        if (contentId is not null)
        {
            queryParams["utm_content"] = contentId;
        }

        return url + QueryString.Create(queryParams);
    }

    public static string GetArkanisGitHubLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo(ApplicationConstants.GitHubRepositoryUrl, contentId);

    public static string GetArkanisGitHubReportBugLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo(
            $"{ApplicationConstants.GitHubRepositoryUrl}/issues/new",
            contentId,
            new Dictionary<string, string>
            {
                ["template"] = "bug_report.yml",
            }
        );

    public static string GetUexLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo(UexConstants.WebBaseUrl, contentId);

    public static string GetUexAccountSettingsLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo($"{UexConstants.WebBaseUrl}/account/home/tab/account_main/", contentId);

    public static string GetUexUserLink(string username, string? contentId = null)
        => AddAttributionGoogleAnalyticsTo($"{UexConstants.WebBaseUrl}/@{username}", contentId);

    public static string? GetUexAssetUrl(string? imageUrl)
        => imageUrl?.Replace(UexConstants.AssetsBaseUrl, UexConstants.AssetsPreferredUrl);

    public static string GetErkulLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo("https://www.erkul.games/", contentId);

    public static string GetRegolithLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo("https://regolith.rocks/", contentId);

    public static string GetCitizenIdLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo("https://citizenid.space/", contentId);

    public static string GetFleetYardsLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo("https://fleetyards.net/", contentId);

    public static string GetMedRunnerLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo("https://medrunner.space/", contentId);

    public static string GetMedRunnerTosLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo("https://medrunner.space/terms-of-service", contentId);

    public static string GetMedRunnerPortalLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo("https://portal.medrunner.space/", contentId);

    public static string GetMedRunnerPortalProfileLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo("https://portal.medrunner.space/profile", contentId);

    public static string GetDiscordUrl(string contentId)
        => AddAttributionGoogleAnalyticsTo("https://discord.com", contentId);
}
