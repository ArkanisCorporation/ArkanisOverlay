namespace Arkanis.Overlay.Infrastructure.Services;

using MudBlazor;

public class NotificationService
{
    public event Action<string, Severity>? NotificationRequested;
    public void ShowNotification(string message, Severity severity) => NotificationRequested?.Invoke(message, severity);
}
