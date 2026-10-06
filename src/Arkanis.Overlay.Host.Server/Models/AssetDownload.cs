namespace Arkanis.Overlay.Host.Server.Models;

public class AssetDownload
{
    public static AssetDownload Missing
        => AssetDownloadMissing.Instance;
}

public class AssetDownloadMissing : AssetDownload
{
    private AssetDownloadMissing()
    {
    }

    public static AssetDownload Instance { get; } = new AssetDownloadMissing();
}

public class ReleaseAssetDownload : AssetDownload
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string BrowserDownloadUrl { get; init; }
}
