namespace Arkanis.Overlay.Components.Services;

using System.Collections.Concurrent;
using Domain.Abstractions.Services;
using Domain.Models.Analytics;
using Microsoft.Extensions.Logging;

public class GlobalAnalyticsReporter(ILogger<GlobalAnalyticsReporter> logger) : IGlobalAnalyticsReporter
{
    private readonly ConcurrentQueue<AnalyticsEvent> _queuedEvents = [];
    private IAnalyticsEventReporter? _reporter;

    public async Task LinkReporterAsync(IAnalyticsEventReporter eventReporter)
    {
        _reporter = eventReporter;
        logger.LogDebug("Emptying analytics event queue of {AnalyticsEventCount}", _queuedEvents.Count);
        while (_queuedEvents.TryDequeue(out var @event))
        {
            await _reporter.TrackEventAsync(@event);
        }
    }

    public async Task TrackEventAsync(AnalyticsEvent analyticsEvent)
    {
        if (_reporter is not null)
        {
            await _reporter.TrackEventAsync(analyticsEvent);
        }
        else
        {
            logger.LogDebug("Enqueuing analytics event: {AnalyticsEventName}", analyticsEvent.EventName);
            _queuedEvents.Enqueue(analyticsEvent);
        }
    }

    public async Task TrackNavigationAsync(string uri)
        => await (_reporter?.TrackNavigationAsync(uri) ?? Task.CompletedTask);
}
