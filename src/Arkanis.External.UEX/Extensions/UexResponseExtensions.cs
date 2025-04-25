namespace Arkanis.External.UEX.Extensions;

using System.Net.Http.Headers;
using Abstractions;

public static class UexResponseExtensions
{
    public static HttpResponseHeaders CreateResponseHeaders(this UexApiResponse response)
    {
        var httpResponse = new HttpResponseMessage();
        foreach (var headerPair in response.Headers)
        {
            httpResponse.Headers.TryAddWithoutValidation(headerPair.Key, headerPair.Value);
        }

        return httpResponse.Headers;
    }

    public static DateTimeOffset GetRequestTime(this HttpResponseHeaders responseHeaders, DateTimeOffset? timeFallback = null)
    {
        timeFallback ??= DateTimeOffset.UtcNow;
        return responseHeaders.Date ?? timeFallback.Value;
    }

    public static DateTimeOffset GetCacheUntil(this HttpResponseHeaders responseHeaders, TimeSpan? cacheTimeFallback = null, double factor = 1.0)
    {
        cacheTimeFallback ??= TimeSpan.FromMinutes(30);
        cacheTimeFallback *= factor;

        var maxCacheAge = responseHeaders.CacheControl?.MaxAge ?? cacheTimeFallback.Value;
        return responseHeaders.GetRequestTime() + maxCacheAge;
    }
}
