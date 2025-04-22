namespace Arkanis.Overlay.Application.Workers;

using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Microsoft.Extensions.Logging;

public class WindowTracker
{
    private static Dictionary<IntPtr, WINEVENTPROC> _registeredHooksDictionary = new();
    private HWND _currentWindowHWnd;

    private Thread? _thread;

    public WindowTracker(ILogger<WindowTracker> logger)
    {
        Logger = logger;
        WindowClass = Constants.WindowClass;
        WindowName = Constants.WindowName;

        WindowFound += OnWindowFound;
        WindowLost += OnWindowLost;
    }

    private ILogger Logger { get; }

    private string? WindowClass { get; }
    private string? WindowName { get; }

    /**
     * Raised when the window was found.
     */
    internal event EventHandler<HWND>? WindowFound;

    /**
     * Raised when the window was lost. This means that it is likely to be destroyed.
     */
    public event EventHandler? WindowLost;

    /**
     * Raised when the window focus was changed.
     * @param focused True if the window is focused, false if it is not.
     */
    public event EventHandler<bool>? WindowFocusChanged;

    /**
     * Raised when the window position changed.
     * @param position The new position of the window.
     */
    public event EventHandler<Point>? WindowPositionChanged;

    /**
     * Raised when the window size changed.
     * @param size The new size of the window.
     */
    public event EventHandler<Size>? WindowSizeChanged;

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
        var hWnd = FindWindow();

        if (hWnd != HWND.Null)
        {
            _currentWindowHWnd = hWnd;
            WindowFound?.Invoke(this, hWnd);
        }
        else
        {
            RegisterWinEventHook
            (
                PInvoke.EVENT_OBJECT_CREATE,
                PInvoke.EVENT_OBJECT_CREATE,
                Handler_WindowCreated,
                0,
                0,
                PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS
            );
        }

        // this thread needs a message loop
        // see: https://stackoverflow.com/a/2223270/4161937
        while (PInvoke.GetMessage(out var msg, HWND.Null, 0, 0))
        {
            PInvoke.TranslateMessage(in msg);
            PInvoke.DispatchMessage(in msg);
        }
    }

    private void RegisterWinEventHook(
        uint eventMin,
        uint eventMax,
        // SafeHandle hmodWinEventProc,
        WINEVENTPROC pfnWinEventProc,
        uint idProcess,
        uint idThread,
        uint dwFlags
    )
    {
        var eventHookHandle = PInvoke.SetWinEventHook
        (
            eventMin,
            eventMax,
            null,
            WinEventDelegate,
            idProcess,
            idThread,
            dwFlags
        );

        _registeredHooksDictionary.Add(eventHookHandle.DangerousGetHandle(), pfnWinEventProc);
    }

    private static void WinEventDelegate(
        HWINEVENTHOOK hWinEventHook,
        uint @event,
        HWND hWnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime
    )
    {
        if (_registeredHooksDictionary.TryGetValue(hWinEventHook, out var hookHandler))
        {
            hookHandler(hWinEventHook, @event, hWnd, idObject, idChild, idEventThread, dwmsEventTime);
            return;
        }

        // Received event for unknown hook / handler - this should never happen,
        // but it is safer to unhook to be sure.
        UnhookWinEvent(hWinEventHook);
    }

    private static void UnhookWinEvent(HWINEVENTHOOK hWinEventHook)
    {
        var success = PInvoke.UnhookWinEvent(hWinEventHook);
        if (!success)
        {
            // Logger.LogWarning("Failed to unhook win event hook: {hWinEventHook}", hWinEventHook);
            Console.WriteLine("Failed to unhook win event hook: {0}", hWinEventHook);
        }
    }

    private HWND FindWindow()
    {
        var result = PInvoke.FindWindow(WindowClass, WindowName);
        return result;
    }

    private static string GetClassName(HWND hWnd)
    {
        if (hWnd == 0)
        {
            return string.Empty;
        }

        Span<char> className = stackalloc char[256];
        PInvoke.GetClassName(hWnd, className);
        return new string(className);
    }

    private static string GetWindowText(HWND hWnd)
    {
        if (hWnd == 0)
        {
            return string.Empty;
        }

        var length = PInvoke.GetWindowTextLength(hWnd);
        Span<char> windowText = stackalloc char[length];
        PInvoke.GetWindowText(hWnd, windowText);
        return new string(windowText);
    }

    private Size GetWindowSize()
    {
        PInvoke.GetClientRect(_currentWindowHWnd, out var result);
        return new Size(result.Width, result.Height);
    }

    private Point GetWindowPosition()
    {
        var point = new Point(0, 0);
        var success = PInvoke.ClientToScreen(_currentWindowHWnd, ref point);
        if (success)
        {
            return point;
        }

        Logger.LogWarning("Failed to get window position");
        return new Point(0, 0); // return value might be zero
    }

    private bool IsWindowFocussed()
    {
        var activeWindowHWnd = PInvoke.GetForegroundWindow();

        return activeWindowHWnd == _currentWindowHWnd && activeWindowHWnd != HWND.Null;
    }

    private void OnWindowFound(object? sender, HWND hWnd)
    {
        Logger.LogDebug("Window found.");

        // emit initial state
        var windowSize = GetWindowSize();
        WindowSizeChanged?.Invoke(this, windowSize);

        var windowPosition = GetWindowPosition();
        WindowPositionChanged?.Invoke(this, windowPosition);

        var windowFocussed = IsWindowFocussed();
        WindowFocusChanged?.Invoke(this, windowFocussed);

        var threadId = PInvoke.GetWindowThreadProcessId(_currentWindowHWnd, out var processId);

        // register event listeners for window changes
        RegisterWinEventHook
        (
            PInvoke.EVENT_OBJECT_DESTROY,
            PInvoke.EVENT_OBJECT_DESTROY,
            Handler_WindowDestroyed,
            processId,
            threadId,
            PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS
        );

        RegisterWinEventHook
        (
            PInvoke.EVENT_OBJECT_LOCATIONCHANGE,
            PInvoke.EVENT_OBJECT_LOCATIONCHANGE,
            Handler_WindowMovedOrResized,
            processId,
            threadId,
            PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS
        );

        // RegisterWinEventHook(
        //     PInvoke.EVENT_OBJECT_FOCUS,
        //     PInvoke.EVENT_OBJECT_FOCUS,
        //     Handler_WindowFocused,
        //     0, // not needed, we need to know if our window has been unfocused
        //     0, // not needed, we need to know if our window has been unfocused
        //     // PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS
        //     PInvoke.WINEVENT_OUTOFCONTEXT
        // );
    }

    private void OnWindowLost(object? sender, EventArgs eventArgs)
    {
        Logger.LogDebug("Window lost.");

        // unhook all current event listeners
        foreach (HWINEVENTHOOK registeredHWinEventHook in _registeredHooksDictionary.Keys)
        {
            UnhookWinEvent(registeredHWinEventHook);
            _registeredHooksDictionary.Remove(registeredHWinEventHook);
        }

        // just to be safe (should, ideally, be completely redundant and a waste of a re-allocation)
        _registeredHooksDictionary = new Dictionary<IntPtr, WINEVENTPROC>();

        // reset window handle
        _currentWindowHWnd = default;

        // start waiting for new window
        RegisterWinEventHook
        (
            PInvoke.EVENT_OBJECT_CREATE,
            PInvoke.EVENT_OBJECT_CREATE,
            Handler_WindowCreated,
            0,
            0,
            PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS
        );
    }


    private void Handler_WindowCreated(
        HWINEVENTHOOK hWinEventHook,
        uint @event,
        HWND hWnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime
    )
    {
        if (hWnd == 0)
        {
            Logger.LogWarning
            (
                "Received window created event but window hWnd is 0: {hWinEventHook}",
                hWinEventHook
            );
            return;
        }

        var windowClass = GetClassName(hWnd);
        var windowTitle = GetWindowText(hWnd);

        if (!(windowClass == WindowClass && windowTitle == WindowName))
        {
            return;
        }

        Logger.LogDebug
        (
            "Window found: WindowTitle = '{windowTitle}', WindowClass = '{windowClass}'",
            windowTitle,
            windowClass
        );

        // update current window handle - other functions implicitly rely on this!
        _currentWindowHWnd = hWnd;

        // stop listening for WindowCreated events
        UnhookWinEvent(hWinEventHook);

        WindowFound?.Invoke(this, hWnd);
    }

    private void Handler_WindowDestroyed(
        HWINEVENTHOOK hWinEventHook,
        uint @event,
        HWND hWnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime
    )
    {
        // hWnd will be Null / 0 because there is no window handle
        // for a destroyed window :)
        // if (hWnd == HWND.Null) return;

        Logger.LogDebug("Window destroyed.");

        WindowLost?.Invoke(this, EventArgs.Empty);
    }

    private void Handler_WindowMovedOrResized(
        HWINEVENTHOOK hWinEventHook,
        uint @event,
        HWND hWnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime
    )
    {
        // safety precaution
        if (hWnd == HWND.Null)
        {
            return;
        }

        WindowPositionChanged?.Invoke(this, GetWindowPosition());
        WindowSizeChanged?.Invoke(this, GetWindowSize());
    }

    public bool IsWindowFocused()
    {
        var hWnd = PInvoke.GetForegroundWindow();
        var isFocused = _currentWindowHWnd != HWND.Null && hWnd == _currentWindowHWnd;

#if DEBUG
        var windowTitle = GetWindowText(hWnd);
        // allows for convenient debugging
        // this way the DevTools window counts as the window being focused
        isFocused |= Debugger.IsAttached && windowTitle.StartsWith("DevTools");
#endif

        return isFocused;
    }

//     private void Handler_WindowFocused(
//         HWINEVENTHOOK hWinEventHook,
//         uint @event,
//         HWND hWnd,
//         int idObject,
//         int idChild,
//         uint idEventThread,
//         uint dwmsEventTime)
//     {
//         // safety precaution
//         // if (hWnd == HWND.Null) return;
//
//         // var isFocused = hWnd == _currentWindowHWnd;
//         // sometimes there is an eroneous detected focus change
//         // if the above check is used, the below check works 100% of the time
//         var isFocused = PInvoke.GetForegroundWindow() == _currentWindowHWnd;
//
// #if DEBUG
//         var windowTitle = GetWindowText(hWnd);
//         // allows for convenient debugging
//         // this way the DevTools window counts as the window being focused
//         isFocused |= Debugger.IsAttached && windowTitle.StartsWith("DevTools");
// #endif
//
//         // Logger.LogDebug(
//         //     "Window focus changed: {isFocused} => Event: {event} - hWnd: {hWnd} - idObject: {idObject} - idChild: {idChild} - idEventThread: {idEventThread} - dwmsEventTime: {dwmsEventTime} - WindowTitle: '{windowTitle}'",
//         //     isFocused, @event, (IntPtr)hWnd, idObject, idChild, idEventThread, dwmsEventTime, windowTitle);
//         WindowFocusChanged?.Invoke(this, isFocused);
//     }
}
