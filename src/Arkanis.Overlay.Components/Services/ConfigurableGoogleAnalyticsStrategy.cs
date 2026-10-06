namespace Arkanis.Overlay.Components.Services;

using Blazor.Analytics;
using Blazor.Analytics.GoogleAnalytics;
using Common;
using Microsoft.Extensions.Hosting;

public sealed class ConfigurableGoogleAnalyticsStrategy : IAnalytics
{
    private readonly GoogleAnalyticsStrategy _strategy;

    public ConfigurableGoogleAnalyticsStrategy(GoogleAnalyticsStrategy strategy, IHostEnvironment hostEnvironment)
    {
        _strategy = strategy;
        _strategy.Configure(ApplicationConstants.GoogleAnalyticsTrackingId, !hostEnvironment.IsProduction());
    }

    /// <summary>
    ///     Configures global tracking configuration properties.
    /// </summary>
    /// <remarks>
    ///     Performs JavaScript interop calls.
    ///     Must only be called in the <c>AfterRenderAsync</c> component lifecycle hook.
    /// </remarks>
    /// <param name="globalConfigData">Configuration data</param>
    /// <returns>A task that finishes when the tracking interop is configured and initialized</returns>
    public Task ConfigureGlobalConfigData(Dictionary<string, object> globalConfigData)
        => _strategy.ConfigureGlobalConfigData(globalConfigData);

    public void ConfigureGlobalEventData(Dictionary<string, object> globalEventData)
        => _strategy.ConfigureGlobalEventData(globalEventData);

    public Task TrackNavigation(string uri)
        => _strategy.TrackNavigation(uri);

    public Task TrackEvent(string eventName, string? eventCategory = null, string? eventLabel = null, int? eventValue = null)
        => _strategy.TrackEvent(eventName, eventCategory, eventLabel, eventValue);

    public Task TrackEvent(string eventName, int eventValue, string? eventCategory = null, string? eventLabel = null)
        => _strategy.TrackEvent(eventName, eventValue, eventCategory, eventLabel);

    public Task TrackEvent(string eventName, object eventData)
        => _strategy.TrackEvent(eventName, eventData);

    public void Enable()
        => _strategy.Enable();

    public void Disable()
        => _strategy.Disable();
}
