namespace Arkanis.Overlay.Host.Desktop.Workers;

using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Uwp.Notifications;

/// <summary>
///     See:
///     https://learn.microsoft.com/en-us/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop#step-4-handling-uninstallation
/// </summary>
public class WindowsToastManagerUninstall : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        ToastNotificationManagerCompat.Uninstall();
        return Task.CompletedTask;
    }
}
