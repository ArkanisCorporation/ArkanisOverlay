namespace Arkanis.Overlay.Host.Desktop.Helpers;

using Common;
using Common.Models;
using Velopack.Sources;

public static class UpdateHelper
{
    private const string? AccessToken = null;

    public static IUpdateSource CreateSourceFor(UpdateChannel channel)
    {
        var allowPreReleases = channel.IsUnstable;
        return new GithubSource(ApplicationConstants.GitHubRepositoryUrl, AccessToken, allowPreReleases);
    }
}
