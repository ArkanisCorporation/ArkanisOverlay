namespace Arkanis.Overlay.Host.Desktop.Services;

using Common;
using Common.Abstractions;
using Common.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

public class WindowsCustomProtocolHandlerManager(
    IUserPreferencesProvider userPreferencesProvider,
    ILogger<WindowsCustomProtocolHandlerManager> logger
) : IHostedService
{
    /// <remarks>
    /// Kept for backwards compatibility with older versions that used incorrect casing.
    /// Used to clean up old registry entries.
    /// </remarks>
    private const string OldProtocolHandlerKeyPath = @$"Software\Classes\{ApplicationConstants.Protocol.OldSchema}";
    /// <remarks>
    /// The registry key path for our custom protocol handler.
    /// Relative to <c>HKEY_CURRENT_USER</c>.
    /// </remarks>
    private static readonly string ProtocolHandlerKeyPath = @$"Software\Classes\{ApplicationConstants.Protocol.Schema}";

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

    /// <summary>
    /// Registers our custom protocol handler in the registry.
    /// <see href="https://learn.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/platform-apis/aa767914(v=vs.85)">MSDN</see>
    /// </summary>
    private static void EnableProtocolHandler()
    {
        // Clean up any existing keys with incorrect casing
        using var oldProtocolHandlerKey = Registry.CurrentUser.OpenSubKey(OldProtocolHandlerKeyPath, true);
        oldProtocolHandlerKey?.DeleteSubKeyTree("");

        using var protocolHandlerKey = Registry.CurrentUser.OpenSubKey(ProtocolHandlerKeyPath, true)
                                       ?? Registry.CurrentUser.CreateSubKey(ProtocolHandlerKeyPath);

        protocolHandlerKey.SetValue(null, $"URL:{ApplicationConstants.ApplicationName} Protocol");
        protocolHandlerKey.SetValue(UrlProtocolKeyName, string.Empty);

        using var applicationKey = protocolHandlerKey.OpenSubKey(ApplicationKeySubPath, true)
                                   ?? protocolHandlerKey.CreateSubKey(ApplicationKeySubPath);

        applicationKey.SetValue(AppNameKeyName, ApplicationConstants.ApplicationName);

        using var handlerCommandKey = protocolHandlerKey.OpenSubKey(HandlerKeySubPath, true)
                                      ?? protocolHandlerKey.CreateSubKey(HandlerKeySubPath);

        handlerCommandKey.SetValue(null, $"\"{Application.ExecutablePath}\" --{ApplicationConstants.Args.HandleUrl} \"%1\"");
    }

    private static void DisableProtocolHandler()
    {
        using var protocolHandlerKey = Registry.CurrentUser.OpenSubKey(ProtocolHandlerKeyPath, true);

        protocolHandlerKey?.DeleteSubKeyTree("");
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
