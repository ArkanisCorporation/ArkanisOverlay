namespace Arkanis.Overlay.Infrastructure.Services;

using System.Text.Json;
using System.Text.Json.Serialization;
using Common;
using Common.Converters.Json;
using Common.Models.Keyboard;
using Common.Options;
using Domain.Abstractions.Services;
using Domain.Models.Analytics;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

[UsedImplicitly]
public class UserPreferencesJsonFileManager(IGlobalAnalyticsReporter analyticsReporter, ILogger<UserPreferencesJsonFileManager> logger)
    : IUserPreferencesManager
{
    private readonly JsonSerializerOptions _options = new()
    {
        IgnoreReadOnlyProperties = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters =
        {
            new KeyboardShortcut.JsonConverter(),
            new RegionInfoJsonConverter(),
            new CultureInfoJsonConverter(),
            new UpdateChannelConverter(),
            new JsonStringEnumConverter(),
        },
    };

    private static FileInfo PreferencesFileInfo
        => new(Path.Combine(ApplicationConstants.ApplicationDataDirectory.FullName, "userPreferences.json"));

    public UserPreferences CurrentPreferences { get; private set; } = new();

    public event EventHandler<UserPreferences>? ApplyPreferences;

    public event EventHandler<UserPreferences>? UpdatePreferences;

    public async Task LoadUserPreferencesAsync()
    {
        if (!PreferencesFileInfo.Exists)
        {
            await SaveAndApplyUserPreferencesAsync(CurrentPreferences).ConfigureAwait(false);
            return;
        }

        UserPreferences? userPreferences = null;
        logger.LogDebug("Loading existing user preferences from {FilePath}", PreferencesFileInfo.FullName);

        try
        {
            await using var fileStream = PreferencesFileInfo.OpenRead();
            userPreferences = await JsonSerializer.DeserializeAsync<UserPreferences>(fileStream, _options).ConfigureAwait(false);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Failed to properly load user preferences from {FilePath}", PreferencesFileInfo.FullName);
        }

        CurrentPreferences = userPreferences ?? CurrentPreferences;
        UpdatePreferences?.Invoke(this, CurrentPreferences);

        await SaveAndApplyUserPreferencesAsync(CurrentPreferences);
    }

    public async Task SaveAndApplyUserPreferencesAsync(UserPreferences userPreferences)
    {
        await TrackFeatureChangesAsync(userPreferences);

        CurrentPreferences = userPreferences;
        ApplyPreferences?.Invoke(this, CurrentPreferences);

        try
        {
            logger.LogDebug("Saving new user preferences to {FilePath}", PreferencesFileInfo.FullName);
            await using var fileStream = PreferencesFileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(fileStream, CurrentPreferences, _options).ConfigureAwait(false);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Failed to properly save user preferences to {FilePath}", PreferencesFileInfo.FullName);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not save preferences to {FilePath}", PreferencesFileInfo.FullName);
        }
    }

    private async Task TrackFeatureChangesAsync(UserPreferences @new)
    {
        await TrackIfChangedAsync(x => x.BlurBackground, FeatureUsageStateChangedEvent.BlurBackground);
        await TrackIfChangedAsync(x => x.TerminateOnGameExit, FeatureUsageStateChangedEvent.TerminateWithGame);
        await TrackIfChangedAsync(x => x.AutoStartWithBoot, FeatureUsageStateChangedEvent.AutoStart);
        await TrackIfChangedAsync(x => x.DisableAnalytics, newValue => FeatureUsageStateChangedEvent.Analytics(!newValue));

        return;

        async Task TrackIfChangedAsync(Func<UserPreferences, bool> isEnabled, Func<bool, AnalyticsEvent> createEvent)
        {
            var newValue = isEnabled(@new);
            if (newValue != isEnabled(CurrentPreferences))
            {
                await analyticsReporter.TrackEventAsync(createEvent(newValue));
            }
        }
    }
}
