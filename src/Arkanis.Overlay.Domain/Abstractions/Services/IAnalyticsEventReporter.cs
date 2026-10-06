namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models.Analytics;

/// <summary>
///     An analytics reporter which can not be generally used in global singleton context.
/// </summary>
public interface IAnalyticsEventReporter
{
    Task TrackEventAsync(AnalyticsEvent analyticsEvent);

    Task TrackNavigationAsync(string uri);
}
