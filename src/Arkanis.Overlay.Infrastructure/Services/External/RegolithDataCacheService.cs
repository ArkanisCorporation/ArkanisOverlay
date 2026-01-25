namespace Arkanis.Overlay.Infrastructure.Services.External;

using System.Text.Json;
using Common;
using Microsoft.Extensions.Logging;
using Overlay.External.Regolith.Abstractions;
using Overlay.External.Regolith.Models;

/// <summary>
/// Caches Regolith API data locally with a 7-day expiration.
/// </summary>
public class RegolithDataCacheService(
    IRegolithApiClient apiClient,
    ILogger<RegolithDataCacheService> logger
)
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromDays(7);
    private static readonly string CacheDirectory = Path.Combine(ApplicationConstants.ApplicationDataDirectory.FullName, "RegolithCache");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    private RegolithLookups? _lookupsCache;
    private Dictionary<string, RegolithSurveyData?>? _surveyDataCache;

    /// <summary>
    /// Gets the lookups data, using cache if available and not expired.
    /// </summary>
    public async Task<RegolithLookups?> GetLookupsAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && _lookupsCache is not null)
        {
            return _lookupsCache;
        }

        var cacheFile = GetCacheFilePath("lookups.json");

        if (!forceRefresh && TryLoadFromCache<RegolithLookups>(cacheFile, out var cached))
        {
            _lookupsCache = cached;
            return cached;
        }

        logger.LogInformation("Fetching fresh lookups data from Regolith API");
        var data = await apiClient.GetLookupsAsync();

        if (data is not null)
        {
            SaveToCache(cacheFile, data);
            _lookupsCache = data;
        }

        return data;
    }

    /// <summary>
    /// Gets survey data, using cache if available and not expired.
    /// </summary>
    public async Task<RegolithSurveyData?> GetSurveyDataAsync(string dataName, string epoch, bool forceRefresh = false)
    {
        var cacheKey = $"{dataName}_{epoch}";

        _surveyDataCache ??= new Dictionary<string, RegolithSurveyData?>();

        if (!forceRefresh && _surveyDataCache.TryGetValue(cacheKey, out var memoryCached))
        {
            return memoryCached;
        }

        var cacheFile = GetCacheFilePath($"survey_{cacheKey}.json");

        if (!forceRefresh && TryLoadFromCache<RegolithSurveyData>(cacheFile, out var cached))
        {
            _surveyDataCache[cacheKey] = cached;
            return cached;
        }

        logger.LogInformation("Fetching fresh survey data '{DataName}' (epoch: {Epoch}) from Regolith API", dataName, epoch);
        var data = await apiClient.GetSurveyDataAsync(dataName, epoch);

        if (data is not null)
        {
            SaveToCache(cacheFile, data);
            _surveyDataCache[cacheKey] = data;
        }

        return data;
    }

    /// <summary>
    /// Clears all cached data and forces a refresh on next request.
    /// </summary>
    public void ClearCache()
    {
        _lookupsCache = null;
        _surveyDataCache?.Clear();

        try
        {
            if (Directory.Exists(CacheDirectory))
            {
                Directory.Delete(CacheDirectory, true);
                logger.LogInformation("Regolith cache cleared");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to clear Regolith cache directory");
        }
    }

    /// <summary>
    /// Gets the age of the cache for a specific data type.
    /// </summary>
    public TimeSpan? GetCacheAge(string dataType)
    {
        var cacheFile = GetCacheFilePath($"{dataType}.json");
        if (!File.Exists(cacheFile))
        {
            return null;
        }

        var metaFile = cacheFile + ".meta";
        if (!File.Exists(metaFile))
        {
            return null;
        }

        try
        {
            var metaContent = File.ReadAllText(metaFile);
            if (DateTime.TryParse(metaContent, out var cachedAt))
            {
                return DateTime.UtcNow - cachedAt;
            }
        }
        catch
        {
            // Ignore errors reading meta file
        }

        return null;
    }

    private static string GetCacheFilePath(string fileName)
    {
        Directory.CreateDirectory(CacheDirectory);
        return Path.Combine(CacheDirectory, fileName);
    }

    private bool TryLoadFromCache<T>(string cacheFile, out T? data) where T : class
    {
        data = null;

        if (!File.Exists(cacheFile))
        {
            return false;
        }

        var metaFile = cacheFile + ".meta";
        if (!File.Exists(metaFile))
        {
            return false;
        }

        try
        {
            var metaContent = File.ReadAllText(metaFile);
            if (!DateTime.TryParse(metaContent, out var cachedAt))
            {
                return false;
            }

            if (DateTime.UtcNow - cachedAt > CacheExpiration)
            {
                logger.LogInformation("Cache expired for {CacheFile} (cached at {CachedAt})", cacheFile, cachedAt);
                return false;
            }

            var json = File.ReadAllText(cacheFile);
            data = JsonSerializer.Deserialize<T>(json);

            if (data is not null)
            {
                logger.LogDebug("Loaded {Type} from cache (age: {Age})", typeof(T).Name, DateTime.UtcNow - cachedAt);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load cache from {CacheFile}", cacheFile);
        }

        return false;
    }

    private void SaveToCache<T>(string cacheFile, T data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(cacheFile, json);
            File.WriteAllText(cacheFile + ".meta", DateTime.UtcNow.ToString("O"));
            logger.LogDebug("Saved {Type} to cache at {CacheFile}", typeof(T).Name, cacheFile);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to save cache to {CacheFile}", cacheFile);
        }
    }
}
