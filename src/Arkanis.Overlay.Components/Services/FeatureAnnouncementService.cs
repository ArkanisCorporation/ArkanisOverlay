namespace Arkanis.Overlay.Components.Services;

using global::Arkanis.Overlay.Common.Abstractions;

using Domain.Abstractions.Services;

public sealed class FeatureAnnouncementService(
    IUserPreferencesManager preferences,
    IEnumerable<FeatureAnnouncement> announcements,
    TimeProvider timeProvider
)
{
    private Task _presentation = Task.CompletedTask;

    private IReadOnlyList<FeatureAnnouncement> Announcements
        => announcements
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.Id, StringComparer.Ordinal)
            .ToArray();

    public IReadOnlyList<FeatureAnnouncement> Pending
        => Announcements
            .Where(x => x.IsActive(timeProvider.GetUtcNow()) && !preferences.CurrentPreferences.DismissedFeatureAnnouncements.Contains(x.DismissalKey))
            .ToArray();

    public async Task ShowPendingAsync(Func<FeatureAnnouncement, Task<bool>> showAsync, Func<bool> canShow)
    {
        // Queue requests so a replacement layout can resume after the previous host tears down.
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var previous = Interlocked.Exchange(ref _presentation, finished.Task);
        try
        {
            await previous;
            while (canShow() && Pending.FirstOrDefault() is { } announcement)
            {
                // A canceled or interrupted dialog is never recorded as acknowledged.
                if (!await showAsync(announcement))
                {
                    break;
                }

                var current = preferences.CurrentPreferences;
                await preferences.SaveAndApplyUserPreferencesAsync(
                    current with
                    {
                        DismissedFeatureAnnouncements = new HashSet<string>(current.DismissedFeatureAnnouncements, StringComparer.Ordinal)
                        {
                            announcement.DismissalKey,
                        },
                    }
                );
            }
        }
        finally
        {
            finished.TrySetResult();
        }
    }
}
