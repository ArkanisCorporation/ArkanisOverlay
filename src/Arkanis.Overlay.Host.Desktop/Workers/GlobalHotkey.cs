namespace Arkanis.Overlay.Host.Desktop.Workers;

using System.Runtime.InteropServices;
using Domain.Abstractions.Services;
using Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
using PInvoke = Windows.Win32.PInvoke;

/// <summary>
///     Captures global hotkeys.
///     Based on: https://learn.microsoft.com/en-us/archive/blogs/toub/low-level-keyboard-hook-in-c
/// </summary>
public sealed class GlobalHotkey : IHostedService, IDisposable
{
    private static HOOKPROC? _proc;
    private static HHOOK _hookId = HHOOK.Null;

    private readonly ILogger _logger;
    private readonly IUserPreferencesProvider _preferencesProvider;

    private Thread? _thread;

    public GlobalHotkey(IUserPreferencesProvider preferencesProvider, ILogger<GlobalHotkey> logger)
    {
        _preferencesProvider = preferencesProvider;
        _logger = logger;
        _proc = HookProc;
    }

    public void Dispose()
        => Unhook();

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _thread = new Thread(Run)
        {
            // ensures that the application can exit
            // regardless of this thread
            // see: https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.isbackground?view=net-9.0
            IsBackground = true,
        };
        _thread.Start();
        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Invoked when the user-configured hotkey press is registered.
    /// </summary>
    public event EventHandler? ConfiguredHotKeyPressed;

    /// <summary>
    ///     Entry method for Thread.
    ///     Registers window event hooks and executes the message loop.
    /// </summary>
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
            _logger.LogWarning("Failed to set hook; {ErrorCode}", errorCode);
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
            _logger.LogWarning("Failed to unhook; {ErrorCode}", errorCode);
            return;
        }

        _hookId = HHOOK.Null;
    }

    /// <summary>
    ///     LowLevelKeyboardProc
    ///     See: https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelkeyboardproc
    /// </summary>
    /// <param name="nCode">
    ///     If nCode is less than zero, the hook procedure must pass the message to the CallNextHookEx function without further
    ///     processing and should return the value returned by CallNextHookEx.
    /// </param>
    /// <param name="wparam">
    ///     The identifier of the keyboard message.
    ///     This parameter can be one of the following messages: WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP.
    /// </param>
    /// <param name="lparam">A pointer to a KBDLLHOOKSTRUCT structure.</param>
    /// <returns>
    ///     If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx.
    ///     If nCode is greater than or equal to zero, and the hook procedure did not process the message, it is highly
    ///     recommended that you call CallNextHookEx and return the value it returns; otherwise, other applications that have
    ///     installed WH_KEYBOARD_LL hooks will not receive hook notifications and may behave incorrectly as a result.
    ///     If the hook procedure processed the message, it may return a nonzero value to prevent the system from passing the
    ///     message to the rest of the hook chain or the target window procedure.
    /// </returns>
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

        var shortcutPressed = _preferencesProvider.CurrentPreferences.LaunchShortcut.PressedKeys
            .Select(WindowsKeyMap.ToCode)
            .All(IsKeyDown);

        if (!shortcutPressed)
        {
            return;
        }

        _logger.LogDebug("Configured Hotkey pressed");
        ConfiguredHotKeyPressed?.Invoke(null, EventArgs.Empty);
    }

    private static bool IsKeyDown(VIRTUAL_KEY vkCode)
        => (PInvoke.GetAsyncKeyState((int)vkCode) & 0x8000) != 0;
}
