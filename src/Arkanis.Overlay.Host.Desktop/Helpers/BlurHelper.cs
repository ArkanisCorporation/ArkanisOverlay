namespace Arkanis.Overlay.Host.Desktop.Helpers;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Extensions.Logging;
using UI.Windows;
using Windows.Win32.Foundation;

/// <summary>
///     Helper class for setting window composition attributes to enable blur effects.
///     Based on:
///     https://github.com/riverar/sample-win32-acrylicblur/blob/917adc277c7258307799327d12262ebd47fd0308/MainWindow.xaml.cs
/// </summary>
public class BlurHelper(WindowProvider<OverlayWindow> windowProvider, ILogger<BlurHelper> logger)
{
    private const uint BlurOpacity = 0;

    [SuppressMessage(
        "Interoperability",
        "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time"
    )]
    [DllImport("user32.dll")]
    private static extern BOOL SetWindowCompositionAttribute(IntPtr hWnd, ref WindowCompositionAttributeData data);

    public void SetBlurEnabled(bool enabled)
    {
        var windowHelper = new WindowInteropHelper(windowProvider.GetWindow());
        var accentState = enabled
            ? AccentState.ACCENT_ENABLE_BLURBEHIND
            : AccentState.ACCENT_DISABLED;

        var accent = new AccentPolicy
        {
            AccentState = accentState,
            GradientColor = 0x00_00_00_00,
            AnimationId = 0,
            AccentFlags = 0,
        };

        var accentStructSize = Marshal.SizeOf(accent);

        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
            SizeOfData = accentStructSize,
            Data = accentPtr,
        };

        var result = SetWindowCompositionAttribute(windowHelper.Handle, ref data);

        if (!result)
        {
            var errorCode = Marshal.GetLastWin32Error();
            logger.LogWarning("Failed to set window composition attribute; {ErrorCode}", errorCode);
        }

        Marshal.FreeHGlobal(accentPtr);
    }
}

// ReSharper disable InconsistentNaming
internal enum AccentState
{
    ACCENT_DISABLED = 0,
    ACCENT_ENABLE_GRADIENT = 1,
    ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
    ACCENT_ENABLE_BLURBEHIND = 3,
    ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
    ACCENT_INVALID_STATE = 5,
}
// ReSharper enable InconsistentNaming

[StructLayout(LayoutKind.Sequential)]
internal struct AccentPolicy
{
    public AccentState AccentState;
    public uint AccentFlags;
    public uint GradientColor;
    public uint AnimationId;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WindowCompositionAttributeData
{
    public WindowCompositionAttribute Attribute;
    public IntPtr Data;
    public int SizeOfData;
}

internal enum WindowCompositionAttribute
{
    // ...
    WCA_ACCENT_POLICY = 19,
    // ...
}
