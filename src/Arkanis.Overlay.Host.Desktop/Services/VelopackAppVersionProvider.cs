namespace Arkanis.Overlay.Host.Desktop.Services;

using Common.Abstractions.Services;
using NuGet.Versioning;
using Workers;

public class VelopackAppVersionProvider(ArkanisOverlayUpdateManager updateManager) : IAppVersionProvider
{
    public SemanticVersion CurrentVersion
        => updateManager.CurrentVersion ?? new SemanticVersion(0, 0, 1, "unknown");

    public string CurrentVelopackChannelId
        => updateManager.CurrentChannel;

    public DateTimeOffset? AutoUpdateCheckAt
        => UpdateProcess.CheckForUpdatesJob.Trigger.GetFireTimeAfter(DateTimeOffset.Now);
}
