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

    private static void SetExtendedStyle(HWND hwnd, WINDOW_EX_STYLE extendedStyle, bool remove = false)
    {
        var exStyle = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

        var newStyle = remove ?
            exStyle & ~(int)extendedStyle :
            exStyle | (int)extendedStyle;

        var result = PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, newStyle);

        if (result != exStyle)
        {
            Log.Error("Failed to set window style: {Message}", Marshal.GetLastPInvokeErrorMessage());
        }
    }

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
