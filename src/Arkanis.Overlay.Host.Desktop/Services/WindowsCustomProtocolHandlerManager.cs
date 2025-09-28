namespace Arkanis.Overlay.Host.Desktop.Services;

using Common;
using Domain.Abstractions.Services;
using Domain.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

public class WindowsCustomProtocolHandlerManager(
    IUserPreferencesProvider userPreferencesProvider,
    ILogger<WindowsCustomProtocolHandlerManager> logger
) : IHostedService
{
    private const string ProtocolCompany = "ArkanisCorp";
    private const string ProtocolSchema = "Overlay";
    private const string Protocol = $"{ProtocolCompany}-{ProtocolSchema}";

    private const string ProtocolHandlerKeyPath = @$"Software\Classes\{Protocol}";

    private const string ApplicationKeySubPath = "Application";
    private const string HandlerKeySubPath = @"shell\open\command";

    private const string UrlProtocolKeyName = "URL Protocol";
    private const string AppNameKeyName = "ApplicationName";

    private static bool RegisterProtocolHandler
        => true;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        userPreferencesProvider.ApplyPreferences += OnUserApplyPreferences;
        OnUserApplyPreferences(null, userPreferencesProvider.CurrentPreferences);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        userPreferencesProvider.ApplyPreferences -= OnUserApplyPreferences;
        return Task.CompletedTask;
    }

    private static void EnableProtocolHandler()
    {
        using var protocolHandlerKey = Registry.CurrentUser.OpenSubKey(ProtocolHandlerKeyPath, true)
                                       ?? Registry.CurrentUser.CreateSubKey(ProtocolHandlerKeyPath);

        protocolHandlerKey.SetValue(null, $"{ApplicationConstants.ApplicationName} Protocol");
        protocolHandlerKey.SetValue(UrlProtocolKeyName, string.Empty);

        using var applicationKey = protocolHandlerKey.OpenSubKey(ApplicationKeySubPath, true)
                                   ?? protocolHandlerKey.CreateSubKey(ApplicationKeySubPath);

        applicationKey.SetValue(AppNameKeyName, ApplicationConstants.ApplicationName);

        using var handlerCommandKey = protocolHandlerKey.OpenSubKey(HandlerKeySubPath, true)
                                      ?? protocolHandlerKey.CreateSubKey(HandlerKeySubPath);

        handlerCommandKey.SetValue(null, $"\"{Application.ExecutablePath}\" --{ApplicationConstants.ArgNames.HandleUrl} \"%1\"");
    }

    private static void DisableProtocolHandler()
    {
        using var protocolHandlerKey = Registry.CurrentUser.OpenSubKey(ProtocolHandlerKeyPath, true);
        if (protocolHandlerKey is null)
        {
            return;
        }

        protocolHandlerKey.DeleteValue(UrlProtocolKeyName);
        protocolHandlerKey.DeleteSubKeyTree(HandlerKeySubPath);
    }

    private void OnUserApplyPreferences(object? sender, UserPreferences userPreferences)
    {
        try
        {
            if (RegisterProtocolHandler)
            {
                logger.LogInformation("Enabling custom protocol handler");
                EnableProtocolHandler();
            }
            else
            {
                logger.LogInformation("Disabling custom protocol handler");
                DisableProtocolHandler();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enable custom protocol handler");
        }
    }
}
