// ? Namespace is required to extend generated PInvoke class
// ReSharper disable once CheckNamespace

namespace Windows.Win32;

using Foundation;

internal partial class PInvoke
{
    /**
     * Workaround for unsafe pointer access.
     * https://github.com/microsoft/CsWin32/issues/137#issuecomment-1879493081
     */
    internal static unsafe uint GetWindowThreadProcessId(HWND hWnd, out uint processId)
    {
        fixed (uint* lpdwProcessId = &processId)
        {
            return GetWindowThreadProcessId(hWnd, lpdwProcessId);
        }
    }
}
