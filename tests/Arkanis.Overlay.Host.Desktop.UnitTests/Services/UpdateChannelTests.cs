namespace Arkanis.Overlay.Host.Desktop.UnitTests.Services;

using System.Text;
using System.Text.Json;
using Common.Abstractions;
using Common.Models;
using Common.Options;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Overlay.Host.Desktop.Services;
using Shouldly;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

public sealed class UpdateChannelTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("ArkanisOverlay.UpdateTests-");

    [Fact]
    public async Task CurrentFindsANewerNightlyReleaseWithoutSwitchingToPreview()
    {
        using var services = CreateServices(new PreferencesProvider(UpdateChannel.Default), CreateLocator("nightly"));
        var manager = services.GetRequiredService<ArkanisOverlayUpdateManager>();

        var update = await manager.CheckForUpdatesAsync();

        update.ShouldNotBeNull();
        update.TargetFullRelease.Version.ToString().ShouldBe("1.11.0-nightly.1");
        update.TargetFullRelease.FileName.ShouldBe("ArkanisOverlay-1.11.0-nightly.1-nightly-full.nupkg");
    }

    [Theory]
    [InlineData("default", "stable", "stable", false)]
    [InlineData("default", "Stable", "Stable", false)]
    [InlineData("default", "rc", "rc", true)]
    [InlineData("default", "preview", "preview", true)]
    [InlineData("default", "nightly", "nightly", true)]
    [InlineData("default", "future-channel", "future-channel", true)]
    [InlineData("default", null, null, true)]
    [InlineData("stable", "nightly", "stable", false)]
    [InlineData("rc", "stable", "rc", true)]
    [InlineData("nightly", "nightly", "preview", true)]
    public void ResolvesFeedAndPrereleasePolicyTogether(string preferenceId, string? installedChannel,
        string? expectedFeedChannel, bool expectedIncludePrereleases)
    {
        var resolved = UpdateHelper.ResolveChannel(UpdateChannel.ById(preferenceId), installedChannel);

        resolved.FeedChannelId.ShouldBe(expectedFeedChannel);
        resolved.IncludePrereleases.ShouldBe(expectedIncludePrereleases);
    }

    [Theory]
    [InlineData("default", "stable")]
    [InlineData("stable", "nightly")]
    public async Task StableExcludesGithubPrereleasesEvenWhenTheyContainAStableFeed(string preferenceId, string installedChannel)
    {
        using var services = CreateServices(new PreferencesProvider(UpdateChannel.ById(preferenceId)), CreateLocator(installedChannel));

        var update = await services.GetRequiredService<ArkanisOverlayUpdateManager>().CheckForUpdatesAsync();

        update.ShouldNotBeNull();
        update.TargetFullRelease.Version.ToString().ShouldBe("1.10.2");
    }

    [Fact]
    public async Task ExplicitPreviewUsesTheIntentionalPreferenceMapping()
    {
        using var services = CreateServices(new PreferencesProvider(UpdateChannel.ById("nightly")), CreateLocator("nightly"));

        var update = await services.GetRequiredService<ArkanisOverlayUpdateManager>().CheckForUpdatesAsync();

        update.ShouldNotBeNull();
        update.TargetFullRelease.Version.ToString().ShouldBe("9.0.0-dev.1");
        update.TargetFullRelease.FileName.ShouldBe("ArkanisOverlay-9.0.0-dev.1-preview-full.nupkg");
    }

    [Fact]
    public async Task UnknownInstalledChannelFindsItsOwnPrereleaseFeedEvenFromAStableVersion()
    {
        using var services = CreateServices(new PreferencesProvider(UpdateChannel.Default), CreateLocator("future-channel", "1.10.2"));

        var update = await services.GetRequiredService<ArkanisOverlayUpdateManager>().CheckForUpdatesAsync();

        update.ShouldNotBeNull();
        update.TargetFullRelease.Version.ToString().ShouldBe("1.11.0-next.1");
        update.TargetFullRelease.FileName.ShouldBe("ArkanisOverlay-1.11.0-next.1-future-channel-full.nupkg");
    }

    [Fact]
    public async Task EachUpdateManagerUsesOnePreferenceSnapshotAndTheNextManagerSeesChanges()
    {
        var preferences = new PreferencesProvider(UpdateChannel.Default, UpdateChannel.Stable);
        using var services = CreateServices(preferences, CreateLocator("nightly"));
        var first = services.GetRequiredService<ArkanisOverlayUpdateManager>();
        var second = services.GetRequiredService<ArkanisOverlayUpdateManager>();

        var firstUpdate = await first.CheckForUpdatesAsync();
        var secondUpdate = await second.CheckForUpdatesAsync();

        firstUpdate.ShouldNotBeNull();
        firstUpdate.TargetFullRelease.Version.ToString().ShouldBe("1.11.0-nightly.1");
        secondUpdate.ShouldNotBeNull();
        secondUpdate.TargetFullRelease.Version.ToString().ShouldBe("1.10.2");
    }

    public void Dispose()
        => _directory.Delete(true);

    private static ServiceProvider CreateServices(IUserPreferencesProvider preferences, IVelopackLocator locator)
        => new ServiceCollection()
            .AddLogging()
            .AddSingleton(preferences)
            .AddSingleton(locator)
            .AddSingleton<IFileDownloader, ReleaseDownloader>()
            .AddVelopackServices()
            .BuildServiceProvider();

    private TestVelopackLocator CreateLocator(string channel, string version = "1.10.2-nightly.1")
        => new(
            "ArkanisOverlay",
            version,
            _directory.FullName,
            _directory.FullName,
            _directory.FullName,
            null,
            channel,
            localPackage: new VelopackAsset
            {
                PackageId = "ArkanisOverlay",
                Version = SemanticVersion.Parse(version),
                Type = VelopackAssetType.Full,
                FileName = "ArkanisOverlay-1.10.2-nightly.1-nightly-full.nupkg",
            });

    private sealed class PreferencesProvider(UpdateChannel initial, UpdateChannel? next = null) : IUserPreferencesProvider
    {
        private int _reads;

        public UserPreferences CurrentPreferences
            => new() { UpdateChannel = _reads++ == 0 ? initial : next ?? initial };

        public event EventHandler<UserPreferences> ApplyPreferences
        {
            add { }
            remove { }
        }
    }

    private sealed class ReleaseDownloader : IFileDownloader
    {
        public Task<string> DownloadString(string url, IDictionary<string, string>? headers = null, double timeout = 30)
        {
            if (url != "https://api.github.com/repos/ArkanisCorporation/ArkanisOverlay/releases?per_page=10&page=1")
            {
                throw new InvalidOperationException($"Unexpected release-list URL: {url}");
            }

            return Task.FromResult(JsonSerializer.Serialize(new[]
            {
                Release("nightly", true, "2026-10-06T14:06:41Z"),
                Release("preview", true, "2026-10-06T14:10:00Z"),
                Release("stable", false, "2026-09-28T01:00:54Z"),
                Release("stable", true, "2026-10-06T14:11:00Z", "unstable-stable"),
                Release("future-channel", true, "2026-10-06T14:12:00Z"),
            }));
        }

        public Task<byte[]> DownloadBytes(string url, IDictionary<string, string>? headers = null, double timeout = 30)
        {
            var (channel, version) = url switch
            {
                "https://updates.example/nightly/releases.nightly.json" => ("nightly", "1.11.0-nightly.1"),
                "https://updates.example/preview/releases.preview.json" => ("preview", "9.0.0-dev.1"),
                "https://updates.example/stable/releases.stable.json" => ("stable", "1.10.2"),
                "https://updates.example/unstable-stable/releases.stable.json" => ("stable", "9.0.0-rc.1"),
                "https://updates.example/future-channel/releases.future-channel.json" => ("future-channel", "1.11.0-next.1"),
                _ => throw new InvalidOperationException($"Unexpected feed URL: {url}"),
            };

            var feed = JsonSerializer.Serialize(new
            {
                Assets = new[]
                {
                    new
                    {
                        PackageId = "ArkanisOverlay",
                        Version = version,
                        Type = "Full",
                        FileName = $"ArkanisOverlay-{version}-{channel}-full.nupkg",
                        SHA1 = "2ED1D4EBCDB11FA168E336E7DC9045E9BC290693",
                        SHA256 = "935229BB75A17A93BC05D552F1CF341DADC24CAB61CBB88C3611383B8387AB75",
                        Size = 40727429,
                    },
                },
            });
            return Task.FromResult(Encoding.UTF8.GetBytes(feed));
        }

        public Task DownloadFile(string url, string targetFile, Action<int> progress, IDictionary<string, string>? headers = null,
            double timeout = 30, CancellationToken cancelToken = default)
            => throw new InvalidOperationException("Checking for updates must not download or install packages.");

        private static object Release(string channel, bool prerelease, string publishedAt, string? feedLocation = null)
            => new
            {
                name = $"Release in {channel}",
                prerelease,
                published_at = publishedAt,
                assets = new[]
                {
                    new
                    {
                        name = $"releases.{channel}.json",
                        url = $"https://api.github.com/repos/ArkanisCorporation/ArkanisOverlay/releases/assets/{channel}",
                        browser_download_url = $"https://updates.example/{feedLocation ?? channel}/releases.{channel}.json",
                        content_type = "application/json",
                    },
                },
            };
    }
}
