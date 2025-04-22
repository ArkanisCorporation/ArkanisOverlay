namespace Arkanis.Overlay.Application.Workers;

using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
using Microsoft.Extensions.Logging;
using PInvoke = Windows.Win32.PInvoke;

/**
 * Captures global hotkeys.
 * Based on: https://learn.microsoft.com/en-us/archive/blogs/toub/low-level-keyboard-hook-in-c
 */
public class GlobalHotkey : IDisposable
{
    private static HOOKPROC? _proc;
    private static HHOOK _hookId = HHOOK.Null;

    private Thread? _thread;

    public GlobalHotkey(ILogger<GlobalHotkey> logger)
    {
        Logger = logger;
        _proc = HookProc;
    }

    private ILogger Logger { get; }

    /**
     *
     */
    public event EventHandler? TabKeyPressed;

    public void Start()
    {
        _thread = new Thread(Run)
        {
            // ensures that the application can exit
            // regardless of this thread
            // see: https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.isbackground?view=net-9.0
            IsBackground = true,
        };
        _thread.Start();
    }

    /**
     * Entry method for Thread.
     * Registers window event hooks and executes the message loop.
     */
    private void Run()
    {
        SetHook();

        // this thread needs a message loop
        // see: https://stackoverflow.com/a/2223270/4161937
        while (PInvoke.GetMessage(out var msg, HWND.Null, 0, 0))
        {
            PInvoke.TranslateMessage(in msg);
            PInvoke.DispatchMessage(in msg);
        }
    }

    private void SetHook()
    {
        var hook = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, _proc, HINSTANCE.Null, 0);
        if (hook == HHOOK.Null)
        {
            var errorCode = Marshal.GetLastWin32Error();
            Logger.LogWarning("Failed to set hook; {errorCode}", errorCode);
            return;
        }

        _hookId = hook;
    }

    private void Unhook()
    {
        if (_hookId == HHOOK.Null)
        {
            return;
        }

        var result = PInvoke.UnhookWindowsHookEx(_hookId);
        if (!result)
        {
            var errorCode = Marshal.GetLastWin32Error();
            Logger.LogWarning("Failed to unhook; {errorCode}", errorCode);
            return;
        }

        _hookId = HHOOK.Null;
    }

    /**
     * LowLevelKeyboardProc
     * See: See: https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelkeyboardproc
     * @param nCode If nCode is less than zero, the hook procedure must pass the message to the CallNextHookEx function without further processing and should return the value returned by CallNextHookEx.
     * @param wparam The identifier of the keyboard message.
     * This parameter can be one of the following messages: WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP.
     * @param lparam A pointer to a KBDLLHOOKSTRUCT structure.
     */
    private LRESULT HookProc(int nCode, WPARAM wparam, LPARAM lparam)
    {
        if (nCode < 0)
        {
            return PInvoke.CallNextHookEx(_hookId, nCode, wparam, lparam);
        }

        HandleKeyEvent(wparam, lparam, out var handled);
        if (handled)
        {
            return (LRESULT)1;
        }

        return PInvoke.CallNextHookEx(_hookId, nCode, wparam, lparam);
    }

    private void HandleKeyEvent(WPARAM wparam, LPARAM lparam, out bool handled)
    {
        handled = false;
        var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lparam);

        if (wparam != PInvoke.WM_KEYUP && wparam != PInvoke.WM_SYSKEYUP)
        {
            return;
        }
        // if (hookStruct.vkCode != (uint)VIRTUAL_KEY.VK_S) return;

        if (
            !IsKeyDown(VIRTUAL_KEY.VK_LSHIFT)
            || !IsKeyDown(VIRTUAL_KEY.VK_LMENU)
            || !IsKeyDown(VIRTUAL_KEY.VK_S)
        )
        {
            return;
        }

        Logger.LogDebug("Hotkey pressed.");
        TabKeyPressed?.Invoke(null, EventArgs.Empty);
    }

    private bool IsKeyDown(VIRTUAL_KEY vkCode)
        => (PInvoke.GetAsyncKeyState((int)vkCode) & 0x8000) != 0;

    public void Dispose()
        => Unhook();
}
