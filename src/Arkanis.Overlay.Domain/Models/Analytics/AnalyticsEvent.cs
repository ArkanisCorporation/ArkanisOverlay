namespace Arkanis.Overlay.Domain.Models.Analytics;

public abstract record AnalyticsEvent(string EventName, string ModuleName);

public record BuiltInAnalyticsEvent(string EventName) : AnalyticsEvent(EventName, "Arkanis.Overlay");

public abstract record BuiltInFeatureUsageStateChangedEvent(string FeatureName, bool IsEnabled) : BuiltInAnalyticsEvent("feature_usage_state_changed");

public sealed record TerminateWithGameFeatureStateChangedEvent(bool IsEnabled) : BuiltInFeatureUsageStateChangedEvent("overlay__app__terminate", IsEnabled);

public sealed record AutoStartFeatureStateChangedEvent(bool IsEnabled) : BuiltInFeatureUsageStateChangedEvent("overlay__app__autostart", IsEnabled);

public sealed record AnalyticsFeatureStateChangedEvent(bool IsEnabled) : BuiltInFeatureUsageStateChangedEvent("overlay__app__analytics", IsEnabled);

public sealed record BlurFeatureStateChangedEvent(bool IsEnabled) : BuiltInFeatureUsageStateChangedEvent("overlay__blur", IsEnabled);

public sealed record SearchEvent(string Query) : BuiltInAnalyticsEvent("search");

public sealed record OverlayShownEvent() : BuiltInAnalyticsEvent("overlay_shown");

public sealed record OverlayHiddenEvent() : BuiltInAnalyticsEvent("overlay_hidden");

public sealed record GameConnectedEvent(Guid SessionId) : BuiltInAnalyticsEvent("game_connected");

public sealed record GameDisconnectedEvent(Guid SessionId) : BuiltInAnalyticsEvent("game_disconnected");

public sealed record DialogOpenedEvent(string DialogId) : BuiltInAnalyticsEvent(nameof(DialogOpenedEvent));
