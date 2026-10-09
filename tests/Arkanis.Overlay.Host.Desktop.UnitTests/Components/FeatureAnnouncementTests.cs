namespace Arkanis.Overlay.Host.Desktop.UnitTests.Components;

using System.Text.Json;
using Overlay.Common.Converters.Json;
using Overlay.Common.Models.Keyboard;
using Overlay.Common.Options;
using Overlay.Components.Services;
using Overlay.Components.Shared.Dialogs;
using Overlay.Infrastructure.Services;
using Shouldly;

public sealed class FeatureAnnouncementTests
{
    [Fact]
    public async Task ConcurrentPresentationShowsOneModalAtATimeAndPersistsEachDismissal()
    {
        var preferences = new InMemoryUserPreferencesManager();
        var first = Announcement("rescue", priority: 10);
        var second = Announcement("trading");
        var service = new FeatureAnnouncementService(preferences, [second, first], TimeProvider.System);
        var opened = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var dismiss = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var shown = new List<string>();
        var activeCount = 0;
        var maximumActive = 0;

        async Task<bool> ShowAsync(FeatureAnnouncement announcement)
        {
            activeCount++;
            maximumActive = Math.Max(maximumActive, activeCount);
            shown.Add(announcement.Id);
            opened.TrySetResult();
            if (announcement.Id == "rescue") await dismiss.Task;
            activeCount--;
            return true;
        }

        var presentation = service.ShowPendingAsync(ShowAsync, () => true);
        await opened.Task;
        var concurrentPresentation = service.ShowPendingAsync(ShowAsync, () => true);
        shown.ShouldBe(["rescue"]);
        dismiss.SetResult(true);
        await Task.WhenAll(presentation, concurrentPresentation);

        maximumActive.ShouldBe(1);
        shown.ShouldBe(["rescue", "trading"]);
        var options = new JsonSerializerOptions
        {
            IgnoreReadOnlyProperties = true,
            Converters = { new CultureInfoJsonConverter(), new RegionInfoJsonConverter(), new UpdateChannelConverter(), new KeyboardShortcut.JsonConverter() },
        };
        var restored = JsonSerializer.Deserialize<UserPreferences>(JsonSerializer.Serialize(preferences.CurrentPreferences, options), options)!;
        var nextSession = new InMemoryUserPreferencesManager();
        await nextSession.SaveAndApplyUserPreferencesAsync(restored);
        new FeatureAnnouncementService(nextSession, [first, second], TimeProvider.System).Pending.ShouldBeEmpty();
    }

    [Fact]
    public async Task CancellationAndOtherOpenDialogsDoNotDismissAnnouncements()
    {
        var preferences = new InMemoryUserPreferencesManager();
        var service = new FeatureAnnouncementService(preferences, [Announcement("rescue"), Announcement("trading")], TimeProvider.System);
        var shown = new List<string>();
        Task<bool> CancelAsync(FeatureAnnouncement announcement)
        {
            shown.Add(announcement.Id);
            return Task.FromResult(false);
        }

        await service.ShowPendingAsync(CancelAsync, () => false);
        shown.ShouldBeEmpty();
        await service.ShowPendingAsync(CancelAsync, () => true);
        shown.ShouldBe(["rescue"]);
        service.Pending.Count.ShouldBe(2);
    }

    [Fact]
    public async Task OnlyActiveUndismissedRevisionsAreEligible()
    {
        var now = new DateTimeOffset(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
        var preferences = new InMemoryUserPreferencesManager();
        var original = Announcement("rescue");
        var service = new FeatureAnnouncementService(preferences, [original], new FixedTimeProvider(now));
        await service.ShowPendingAsync(_ => Task.FromResult(true), () => true);
        var revised = original with { Revision = 2 };
        var announcements = new[]
        {
            original, revised,
            Announcement("disabled") with { Enabled = false },
            Announcement("future") with { AvailableFrom = now.AddMinutes(1) },
            Announcement("expired") with { AvailableUntil = now },
            Announcement("active") with { AvailableFrom = now, AvailableUntil = now.AddMinutes(1) },
        };

        new FeatureAnnouncementService(preferences, announcements, new FixedTimeProvider(now))
            .Pending.Select(x => x.Id).ShouldBe(["active", "rescue"]);
    }

    [Fact]
    public async Task DismissalPreservesPreferencesChangedWhileDialogWasOpen()
    {
        var preferences = new InMemoryUserPreferencesManager();
        var service = new FeatureAnnouncementService(preferences, [Announcement("rescue")], TimeProvider.System);
        await service.ShowPendingAsync(async _ =>
        {
            await preferences.SaveAndApplyUserPreferencesAsync(preferences.CurrentPreferences with { BlurBackground = true });
            return true;
        }, () => true);

        preferences.CurrentPreferences.BlurBackground.ShouldBeTrue();
        service.Pending.ShouldBeEmpty();
    }

    private static FeatureAnnouncement Announcement(string id, int priority = 0)
        => new() { Id = id, Title = id, ContentComponentType = typeof(AnniversaryDialog), Priority = priority };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
