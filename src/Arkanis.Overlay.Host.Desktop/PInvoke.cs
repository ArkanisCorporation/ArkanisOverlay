// ? Namespace is required to extend generated PInvoke class
// ReSharper disable once CheckNamespace

namespace Windows.Win32;

using System.Threading;
using Foundation;
using UI.WindowsAndMessaging;

internal partial class PInvoke
{
    /// <summary>
    ///     Workaround for unsafe pointer access.
    ///     See: https://github.com/microsoft/CsWin32/issues/137#issuecomment-1879493081
    /// </summary>
    /// <param name="hWnd">Target window handle</param>
    /// <param name="processId">Process ID of the window</param>
    /// <returns></returns>
    internal static unsafe uint GetWindowThreadProcessId(HWND hWnd, out uint processId)
    {
        fixed (uint* lpdwProcessId = &processId)
        {
            return GetWindowThreadProcessId(hWnd, lpdwProcessId);
        }
    }

    public static string? GetClassName(HWND hWnd)
    {
        if (hWnd == 0)
        {
            return string.Empty;
        }

        Span<char> className = stackalloc char[256];
        var length = GetClassName(hWnd, className);

        return SpanToString(className, length);
    }

    public static string? GetWindowText(HWND hWnd)
    {
        if (hWnd == 0)
        {
            return string.Empty;
        }

        var length = GetWindowTextLength(hWnd);
        Span<char> windowText = stackalloc char[length];
        GetWindowText(hWnd, windowText);
        return SpanToString(windowText, length - 1); // ignore trailing null terminator
    }

    public static bool IsTopLevelWindow(HWND hWnd)
    {
        if (hWnd == HWND.Null)
        {
            return false;
        }

        return GetAncestor(hWnd, GET_ANCESTOR_FLAGS.GA_ROOT) == hWnd;
    }

    public static string? GetWindowProcessName(HWND hWnd)
    {
        _ = GetWindowThreadProcessId(hWnd, out var processId);

        using var processSafeHandle = OpenProcess_SafeHandle(
            PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION,
            false,
            processId
        );

        if (processSafeHandle.IsInvalid)
        {
            return null;
        }

        Span<char> processName = stackalloc char[1024];

        var result = GetModuleFileNameEx(processSafeHandle, null, processName);

        return SpanToString(processName, result);
    }

    private static string? SpanToString(Span<char> buffer, uint length)
    {
        if (length == 0 || buffer.IsEmpty)
        {
            return null;
        }

        // we do not need to check for overflow here
        // because `buffer.Length` is always less than or equal to `Int32.MaxValue`.
        //
        // To verify, uncomment below line and see the compiler warning.
        // if (buffer.Length > int.MaxValue) { }

        var safeLength = Convert.ToInt32(Math.Min(buffer.Length, length));

        return new string(buffer[..safeLength]);
    }

    private static string? SpanToString(Span<char> buffer, int length)
    {
        if (length == 0 || buffer.IsEmpty)
        {
            return null;
        }

        var safeLength = Math.Min(buffer.Length, length);

        return new string(buffer[..safeLength]);
    }
}
