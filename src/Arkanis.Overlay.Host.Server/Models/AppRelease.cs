namespace Arkanis.Overlay.Host.Server.Models;

using Octokit;

public class AppRelease
{
    public static AppRelease Missing
        => AppReleaseMissing.Instance;
}

public class AppReleaseMissing : AppRelease
{
    private AppReleaseMissing()
    {
    }

    public static AppRelease Instance { get; } = new AppReleaseMissing();
}

public class GitHubAppRelease : AppRelease
{
    public required Release Release { get; init; }
    public required AssetDownload PortableAsset { get; init; }
    public required AssetDownload InstallerAsset { get; init; }
}
