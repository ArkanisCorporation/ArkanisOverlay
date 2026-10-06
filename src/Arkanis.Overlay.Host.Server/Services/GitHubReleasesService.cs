namespace Arkanis.Overlay.Host.Server.Services;

using Common;
using Microsoft.Extensions.Caching.Memory;
using Models;
using Octokit;

internal class GitHubReleasesService(IMemoryCache memoryCache, ILogger<GitHubReleasesService> logger)
{
    private readonly GitHubClient _client = new(new ProductHeaderValue(ServerHostModule.Namespace));

    public async Task<AppRelease> GetLatestStableDownloadsAsync()
    {
        var releases = await GetReleasesAsync();
        if (releases.FirstOrDefault(x => x.Prerelease == false) is not { } latestRelease)
        {
            latestRelease = releases.FirstOrDefault();
            if (latestRelease is null)
            {
                return AppRelease.Missing;
            }
        }

        return await CreateDownloadsAsync(latestRelease);
    }

    private async Task<AppRelease> CreateDownloadsAsync(Release latestRelease)
        => new GitHubAppRelease
        {
            Release = latestRelease,
            PortableAsset = await CreatePortableAssetDownloadAsync(latestRelease),
            InstallerAsset = await CreateInstallerAssetDownloadAsync(latestRelease),
        };

    private async Task<AssetDownload> CreatePortableAssetDownloadAsync(Release latestRelease)
    {
        if (await GetPortableDownloadAssetAsync(latestRelease) is not { } portableAsset)
        {
            return AssetDownload.Missing;
        }

        return new ReleaseAssetDownload
        {
            Name = portableAsset.Label,
            Version = latestRelease.TagName,
            BrowserDownloadUrl = portableAsset.BrowserDownloadUrl,
        };
    }

    private async Task<AssetDownload> CreateInstallerAssetDownloadAsync(Release latestRelease)
    {
        if (await GetInstallerDownloadAssetAsync(latestRelease) is not { } installerAsset)
        {
            return AssetDownload.Missing;
        }

        return new ReleaseAssetDownload
        {
            Name = installerAsset.Label,
            Version = latestRelease.TagName,
            BrowserDownloadUrl = installerAsset.BrowserDownloadUrl,
        };
    }

    private async Task<ReleaseAsset?> GetPortableDownloadAssetAsync(Release release)
    {
        var assets = await GetReleaseAssetsAsync(release);
        return assets.FirstOrDefault(asset => asset.Name.EndsWith("-Portable.zip", StringComparison.Ordinal));
    }

    private async Task<ReleaseAsset?> GetInstallerDownloadAssetAsync(Release release)
    {
        var assets = await GetReleaseAssetsAsync(release);
        return assets.FirstOrDefault(asset => asset.Name.EndsWith("-Setup.exe", StringComparison.Ordinal));
    }

    private async Task<IReadOnlyList<ReleaseAsset>> GetReleaseAssetsAsync(Release release)
        => await memoryCache.GetOrCreateAsync(
               $"{nameof(GitHubReleasesService)}-{nameof(GetReleaseAssetsAsync)}",
               async entry =>
               {
                   try
                   {
                       entry.SetSlidingExpiration(TimeSpan.FromHours(1));
                       return await _client.Repository.Release.GetAllAssets(
                           ApplicationConstants.GitHubOwner,
                           ApplicationConstants.GitHubRepository,
                           release.Id
                       );
                   }
                   catch (Exception ex)
                   {
                       logger.LogError(ex, "Failed loading available release assets for {ReleaseId}", release.Id);
                       entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                       return [];
                   }
               }
           )
           ?? [];

    private async Task<IReadOnlyList<Release>> GetReleasesAsync()
        => await memoryCache.GetOrCreateAsync(
               $"{nameof(GitHubReleasesService)}-{nameof(GetLatestStableDownloadsAsync)}",
               async entry =>
               {
                   try
                   {
                       entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                       return await _client.Repository.Release.GetAll(ApplicationConstants.GitHubOwner, ApplicationConstants.GitHubRepository);
                   }
                   catch (Exception ex)
                   {
                       logger.LogError(ex, "Failed loading available releases");
                       entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                       return [];
                   }
               }
           )
           ?? [];
}
