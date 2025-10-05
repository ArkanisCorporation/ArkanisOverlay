namespace Arkanis.Overlay.Host.Desktop.Workers;

using Arkanis.Overlay.Domain.Abstractions.Services;
using Microsoft.Extensions.Hosting;

public class GameWindowTrackerOverlayEventRelayService(GameWindowTracker windowTracker, IOverlayEventControls overlayEventControls) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        windowTracker.WindowTrackingChanged += OnWindowTrackerOnWindowTrackingChanged;
        windowTracker.WindowFocusChanged += OnWindowTrackerOnWindowFocusChanged;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    private void OnWindowTrackerOnWindowTrackingChanged(object? _, GameWindowTracker.WindowTrackingInfo info)
    {
        if (info.IsTracked)
        {
            overlayEventControls.OnGameWindowFound();
        }
        else
        {
            overlayEventControls.OnGameWindowLost();
        }
    }

    private void OnWindowTrackerOnWindowFocusChanged(object? _, bool isFocused)
    {
        if (isFocused)
        {
            overlayEventControls.OnGameWindowFocused();
        }
        else
        {
            overlayEventControls.OnGameWindowBlurred();
        }
    }
}
