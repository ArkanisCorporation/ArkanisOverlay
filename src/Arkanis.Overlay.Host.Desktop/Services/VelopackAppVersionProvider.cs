namespace Arkanis.Overlay.Host.Desktop.Services;

using Common.Abstractions;
using NuGet.Versioning;
using Workers;

public class VelopackAppVersionProvider(ArkanisOverlayUpdateManager updateManager) : IAppVersionProvider
{
    public SemanticVersion CurrentVersion
        // Velopack v1 exposes CurrentVersion as its own Velopack.SemanticVersion; the app's
        // canonical version type is NuGet.Versioning.SemanticVersion, so convert at this boundary.
        => updateManager.CurrentVersion is { } version
            ? SemanticVersion.Parse(version.ToString())
            : new SemanticVersion(0, 0, 1, "unknown");

    public string CurrentVelopackChannelId
        => updateManager.CurrentChannel;

    public DateTimeOffset? AutoUpdateCheckAt
        => UpdateProcess.CheckForUpdatesJob.Trigger.GetFireTimeAfter(DateTimeOffset.Now);
}
