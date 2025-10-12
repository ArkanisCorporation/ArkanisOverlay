namespace Arkanis.Overlay.Domain.Abstractions.Services;

/// <summary>
///     A global analytics reporter to be used in any singleton context.
/// </summary>
public interface IGlobalAnalyticsReporter : IAnalyticsEventReporter
{
    /// <summary>
    ///     Tracking reporting may be linked to JavaScript interop calls.
    /// </summary>
    /// <param name="eventReporter">The reporter to be used to send out analytic events</param>
    public Task LinkReporterAsync(IAnalyticsEventReporter eventReporter);
}
