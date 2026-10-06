namespace Arkanis.Overlay.Infrastructure.Services;

using Domain.Abstractions.Services;
using Domain.Models.Analytics;

public class FakeAnalyticsReporter : IGlobalAnalyticsReporter
{
    public Task TrackEventAsync(AnalyticsEvent analyticsEvent)
        => Task.CompletedTask;

    public Task TrackNavigationAsync(string uri)
        => Task.CompletedTask;

    public Task LinkReporterAsync(IAnalyticsEventReporter eventReporter)
        => Task.CompletedTask;
}
