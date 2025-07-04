namespace Arkanis.Overlay.Host.Desktop.Services;

using System.Reflection;
using Infrastructure.Services.Abstractions;
using Microsoft.Win32;

public class WindowsAutoStartStateProvider : ISystemAutoStartStateProvider
{
    /// <summary>
    ///     Checks if the application is set to run at startup by testing if the key exists in the registry
    ///     and if the value matches the current executable path.
    /// </summary>
    public bool IsAutoStartEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(WindowsAutoStartManager.RegistryKeyPath, false);

        if (key?.GetValue(WindowsAutoStartManager.RegistryKeyName) is not string value)
        {
            return false;
        }

        var exePath = WindowsAutoStartManager.ExecutablePath;
        return value == $"\"{exePath}\"";
    }
}
