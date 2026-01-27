namespace Arkanis.Overlay.Common.Abstractions;

using Models;
using NuGet.Versioning;

public interface IAppVersionProvider
{
    public SemanticVersion CurrentVersion { get; }

    public UpdateChannel CurrentUpdateChannel
        => UpdateChannel.ByVelopackChannelId(CurrentVelopackChannelId);

    public string CurrentVelopackChannelId { get; }

    public DateTimeOffset? AutoUpdateCheckAt { get; }
}
