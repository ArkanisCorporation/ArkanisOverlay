namespace Arkanis.Overlay.Infrastructure.Helpers;

using Microsoft.AspNetCore.Http;

public static class ExternalLinkHelper
{
    private static string AddAttributionGoogleAnalyticsTo(string url, string? contentId = null)
    {
        var queryParams = new Dictionary<string, string>
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

    public static string GetUexLink(string? contentId = null)
        => AddAttributionGoogleAnalyticsTo("https://uexcorp.space/", contentId);

    public static string GetUexUserLink(string username, string? contentId = null)
        => AddAttributionGoogleAnalyticsTo($"https://uexcorp.space/@{username}", contentId);
}
