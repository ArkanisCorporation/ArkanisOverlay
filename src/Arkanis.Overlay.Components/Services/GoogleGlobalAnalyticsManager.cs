namespace Arkanis.Overlay.Components.Services;

using Blazor.Analytics;
using Domain.Abstractions.Services;
using Domain.Models.Analytics;
using Microsoft.Extensions.Logging;

public class GoogleAnalyticsEventReporter(
    IAnalytics analytics,
    SharedAnalyticsPropertyProvider analyticsPropertyProvider,
    ILogger<GoogleAnalyticsEventReporter> logger
) : IAnalyticsEventReporter
{
    private const string EventCategoryKey = "event_category";
    private const string EventLabelKey = "event_label";


    public async Task TrackEventAsync(AnalyticsEvent analyticsEvent)
    {
        logger.LogDebug("Reporting event: {AnalyticsEventName}", analyticsEvent.EventName);
        await analytics.TrackEvent(analyticsEvent.EventName, CreateEventData(analyticsEvent));
    }

    public async Task TrackNavigationAsync(string uri)
        => await analytics.TrackNavigation(uri);

    private Dictionary<string, object> CreateEventData(AnalyticsEvent analyticsEvent)
    {
        var eventData = analyticsEvent switch
        {
            BuiltInFeatureUsageStateChangedEvent @event => CreateSpecificEventData(@event),
            DialogOpenedEvent @event => CreateSpecificEventData(@event),
            SearchEvent @event => CreateSpecificEventData(@event),
            _ => [],
        };

        AddCommonEventData(eventData);
        return eventData;
    }

    private void AddCommonEventData(Dictionary<string, object> eventData)
    {
        foreach (var (propertyName, propertyValue) in analyticsPropertyProvider.PropertyItems)
        {
            eventData[propertyName] = propertyValue;
        }
    }

    private static Dictionary<string, object> CreateSpecificEventData(BuiltInFeatureUsageStateChangedEvent @event)
        => new()
        {
            [EventCategoryKey] = "Feature",
            [EventLabelKey] = @event.FeatureName,
            ["value"] = @event.IsEnabled ? 1 : 0,
        };

    private static Dictionary<string, object> CreateSpecificEventData(SearchEvent @event)
        => new()
        {
            ["search_term"] = @event.Query,
        };

    private static Dictionary<string, object> CreateSpecificEventData(DialogOpenedEvent @event)
        => new()
        {
            [EventCategoryKey] = "Dialog",
            [EventLabelKey] = "Opened",
            ["value"] = @event.DialogId,
        };
}
