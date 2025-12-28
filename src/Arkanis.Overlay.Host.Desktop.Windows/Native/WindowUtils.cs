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
    public void SetExtendedStyle(Window window, WindowStyle extendedStyle)
    {
        var iHandle = window.TryGetPlatformHandle();
        if (iHandle is null)
        {
            throw new NotSupportedException("Window does not have a native platform handle.");
        }

        var nativeStyle = ConvertToNativeStyle(extendedStyle);

        var handle = (HWND)iHandle.Handle;
        var exStyle = PInvoke.GetWindowLong(handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        var newStyle = exStyle | (int)nativeStyle;
        var result = PInvoke.SetWindowLong(handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, newStyle);

        if (result != exStyle)
        {
            Log.Error("Failed to set window style: {Message}", Marshal.GetLastPInvokeErrorMessage());
        }
    }

    private static WINDOW_EX_STYLE ConvertToNativeStyle(WindowStyle style)
        => style switch
        {
            WindowStyle.Transparent => WINDOW_EX_STYLE.WS_EX_TRANSPARENT,
            WindowStyle.ToolWindow => WINDOW_EX_STYLE.WS_EX_TOOLWINDOW,
            WindowStyle.Layered => WINDOW_EX_STYLE.WS_EX_LAYERED,
            WindowStyle.NoActivate => WINDOW_EX_STYLE.WS_EX_NOACTIVATE,
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };
}
