namespace Arkanis.Overlay.Host.Desktop.Helpers;

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Serilog;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

public static class WindowUtils
{
    public static void StyleAsToolWindow(Window window)
        => SetExtendedStyle(window, WINDOW_EX_STYLE.WS_EX_TOOLWINDOW);

    public static void StyleAsTransparent(Window window)
        => SetExtendedStyle(window, WINDOW_EX_STYLE.WS_EX_TRANSPARENT);

    public static void StyleAsLayered(Window window)
        => SetExtendedStyle(window, WINDOW_EX_STYLE.WS_EX_LAYERED);

    public static void StyleAsNoActivate(Window window)
        => SetExtendedStyle(window, WINDOW_EX_STYLE.WS_EX_NOACTIVATE);

    internal static void SetExtendedStyle(Window window, WINDOW_EX_STYLE extendedStyle)
    {
        var wndHelper = new WindowInteropHelper(window);

        var exStyle = PInvoke.GetWindowLong((HWND)wndHelper.Handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        var newStyle = exStyle | (int)extendedStyle;
        var result = PInvoke.SetWindowLong((HWND)wndHelper.Handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, newStyle);

        if (result != exStyle)
        {
            Log.Error("Failed to set window style: {Message}", Marshal.GetLastPInvokeErrorMessage());
        }
    }
}
