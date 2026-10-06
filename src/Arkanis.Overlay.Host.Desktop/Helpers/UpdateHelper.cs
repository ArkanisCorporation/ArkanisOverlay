namespace Arkanis.Overlay.Host.Desktop.Helpers;

using Common;
using Common.Abstractions;
using Common.Models;
using Microsoft.Extensions.Logging;
using Services;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

public class UpdateHelper(
    IUserPreferencesProvider preferences,
    IVelopackLocator locator,
    ILogger<UpdateHelper> logger,
    IFileDownloader? downloader = null
)
{
    private const string? AccessToken = null;

    public ArkanisOverlayUpdateManager CreateUpdateManager()
    {
        var selected = preferences.CurrentPreferences.UpdateChannel;
        var resolved = ResolveChannel(selected, locator.Channel);
        var manager = new ArkanisOverlayUpdateManager(CreateSourceFor(resolved), CreateOptionsFor(resolved), locator);

        logger.LogDebug(
            "Configured application updates for preference {UpdatePreference}, installed channel {InstalledChannel}, resolved channel {UpdateChannel}, include GitHub prereleases {IncludePrereleases}",
            selected.InternalId,
            locator.Channel,
            manager.CurrentChannel,
            resolved.IncludePrereleases
        );

        return manager;
    }

    internal static ResolvedUpdateChannel ResolveChannel(UpdateChannel selected, string? installedChannelId)
    {
        if (selected.VelopackChannelId is { } explicitChannelId)
        {
            return new ResolvedUpdateChannel(explicitChannelId, selected.IsUnstable);
        }

        var installedDefinition = UpdateChannel.All.FirstOrDefault(channel =>
            channel.VelopackChannelId is not null && string.Equals(channel.VelopackChannelId, installedChannelId, StringComparison.OrdinalIgnoreCase));

        // Preserve the installed feed ID through channel renames. Unknown channels must
        // still discover prereleases; Velopack only reads assets from that exact feed.
        return new ResolvedUpdateChannel(installedChannelId, installedDefinition?.IsUnstable ?? true);
    }

    private GithubSource CreateSourceFor(ResolvedUpdateChannel channel)
        => new GithubSource(ApplicationConstants.GitHubRepositoryUrl, AccessToken, channel.IncludePrereleases, downloader);

    private static UpdateOptions CreateOptionsFor(ResolvedUpdateChannel channel)
        => new()
        {
            AllowVersionDowngrade = true,
            ExplicitChannel = channel.FeedChannelId,
        };

    internal sealed record ResolvedUpdateChannel(string? FeedChannelId, bool IncludePrereleases);
}
