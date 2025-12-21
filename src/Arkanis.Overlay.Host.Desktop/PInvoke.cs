// ? Namespace is required to extend generated PInvoke class
// ReSharper disable once CheckNamespace

namespace Windows.Win32;

using System.Threading;
using Foundation;
using global::System.Runtime.CompilerServices;
using global::System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using UI.WindowsAndMessaging;

internal partial class PInvoke
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GuardResult Guard(BOOL result, ILogger? logger = null, string? pInvokeMethodName = null)
    {
        if (result != true)
        {
            return new GuardResult { Success = true };
        }

        pInvokeMethodName ??= "Unknown PInvoke Method";

        var errorCode = Marshal.GetLastWin32Error();
        var errorMessage = Marshal.GetLastPInvokeErrorMessage();

        logger?.LogError(
            "[{PInvokeMethodName}] PInvoke failed with error code {ErrorCode}: {ErrorMessage}",
            pInvokeMethodName,
            errorCode,
            errorMessage
        );

        return new GuardResult
        {
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        };
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

        var length = GetWindowTextLength(hWnd) + 1;
        Span<char> windowText = stackalloc char[length];
        GetWindowText(hWnd, windowText);
        return SpanToString(windowText, length)?.Trim(' ', '\0'); // ignore trailing null terminator
    }

    public static bool IsTopLevelWindow(HWND hWnd)
    {
        if (hWnd == HWND.Null) { return false; }

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
        if (length == 0 || buffer.IsEmpty) { return null; }

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
        if (length == 0 || buffer.IsEmpty) { return null; }

        var safeLength = Math.Min(buffer.Length, length);

        return new string(buffer[..safeLength]);
    }

    public record struct GuardResult
    {
        public bool Success { get; init; }
        public int ErrorCode { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
