namespace Arkanis.Overlay.Host.Desktop.Windows.Native;

using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using global::Windows.Win32;
using global::Windows.Win32.Foundation;
using global::Windows.Win32.UI.WindowsAndMessaging;
using Overlay.UI.Native;
using Serilog;

public class WindowUtils : IWindowUtils
{
    private const WINDOW_EX_STYLE ClickThroughStyle = WINDOW_EX_STYLE.WS_EX_TRANSPARENT | WINDOW_EX_STYLE.WS_EX_LAYERED;

    /// <summary>
    /// Allows mouse clicks to pass through the specified window.
    /// </summary>
    /// <param name="window"></param>
    public void EnableClickThrough(Window window)
    {
        var hwnd = GetHwnd(window);
        SetExtendedStyle(hwnd, ClickThroughStyle);
    }

    public void DisableClickThrough(Window window)
    {
        var hwnd = GetHwnd(window);
        SetExtendedStyle(hwnd, ClickThroughStyle, true);
    }

    /// <summary>
    /// Updates the extended window style for the specified window with the given style options.
    /// Can be used to en- or disable e.g. Layered window compositing or transparency.
    /// </summary>
    /// <param name="hwnd">The native Windows window handle. Can be obtained from an Avalonia <see cref="Window"/> using <see cref="GetHwnd"/>.</param>
    /// <param name="extendedStyle">The extended window style flag enum value, one or more values combined using bit-wise OR.</param>
    /// <param name="remove">If true, removes the style instead of adding it. Defaults to false.</param>
    private static void SetExtendedStyle(HWND hwnd, WINDOW_EX_STYLE extendedStyle, bool remove = false)
    {
        var exStyle = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

        var newStyle = remove ? exStyle & ~(int)extendedStyle : exStyle | (int)extendedStyle;

        var result = PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, newStyle);

        if (result != exStyle)
        {
            Log.Error("Failed to set window style: {Message}", Marshal.GetLastPInvokeErrorMessage());
        }
    }

    /// <summary>
    /// Gets the native Windows HWND handle for the specified Avalonia window.
    /// </summary>
    /// <param name="window">The Avalonia window instance.</param>
    /// <returns>The native Windows window handle.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when the Avalonia window does not have a Platform Handle.
    ///     ( <see cref="Window.TryGetPlatformHandle"/> returned <c>null</c> )
    /// </exception>
    private static HWND GetHwnd(Window window)
    {
        var iHandle = window.TryGetPlatformHandle();
        if (iHandle is not { Handle: var handle })
        {
            throw new NotSupportedException("Window does not have a native platform handle.");
        }

        return (HWND)handle;
    }
}
