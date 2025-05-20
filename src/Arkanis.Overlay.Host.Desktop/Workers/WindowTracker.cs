namespace Arkanis.Overlay.Host.Desktop.Workers;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Domain.Abstractions.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.HiDpi;

// /// <summary>
// /// Represents the current state of the window, e.g. if it is minimized, maximized, normal, closed, or lost from tracking.
// /// </summary>
// public enum ExtendedWindowState
// {
//     Minimized,
//     Maximized,
//     Normal,
//     Closed,
//     Lost,
// }

/// <summary>
///     Tracks the target window and raises events for window state changes, window position changes, and window focus
///     changes.
/// </summary>
public sealed class WindowTracker : IHostedService, IDisposable
{
    private const uint WmInvokeAction = PInvoke.WM_USER + 100;
    private const double DefaultDpi = 96.0; // This is official and fixed by Windows logic


    private const string WindowClass = Constants.WindowClass;
    private const string WindowName = Constants.WindowName;
    private static Dictionary<HWINEVENTHOOK, WINEVENTPROC> _registeredHooksDictionary = new();
    private static readonly Dictionary<HWINEVENTHOOK, Thread> ThreadMap = new();

    private readonly ConcurrentQueue<Action> _actionQueue = new();
    private readonly IHostApplicationLifetime _applicationLifetime;

    private readonly ILogger _logger;
    private readonly IUserPreferencesProvider _userPreferencesProvider;

    private HWND _currentWindowHWnd;
    private uint _currentWindowProcessId;
    private uint _currentWindowThreadId;

    private CancellationTokenSource _processExitWatcherCts = new();

    public Size CurrentWindowSize { get; private set; }
    public Point CurrentWindowPosition { get; private set; }


    /// <summary>
    ///     The self-launched thread this class runs on.
    ///     This is needed to be able to stop the thread.
    /// </summary>
    private Thread? _thread;

    private uint _threadId;

    public WindowTracker(
        IHostApplicationLifetime applicationLifetime,
        IUserPreferencesProvider userPreferencesProvider,
        ILogger<WindowTracker> logger
    )
    {
        _applicationLifetime = applicationLifetime;
        _userPreferencesProvider = userPreferencesProvider;
        _logger = logger;

        WindowFound += OnWindowFound;
        ProcessExited += OnProcessExited;
    }

    public void Dispose()
    {
        StopProcessExitWatcher();
        _processExitWatcherCts.Dispose();
    }

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

    // /// <summary>
    // ///
    // /// </summary>
    // /// <remarks>
    // ///     By default, we do not know the current window state, so we have "lost" track of it
    // /// </remarks>
    // private ExtendedWindowState _currentWindowState = ExtendedWindowState.Lost;

    /// <summary>
    ///     Raised when the Star Citizen game window was found.
    /// </summary>
    internal event EventHandler<HWND>? WindowFound;

    public event EventHandler? WindowMinimized;
    public event EventHandler? WindowRestored;
    public event EventHandler? ProcessExited;

    /// <summary>
    ///     Raised when the window focus was changed.
    ///     Reports true if the window is focused, false otherwise.
    /// </summary>
    public event EventHandler<bool>? WindowFocusChanged;

    /// <summary>
    ///     Raised when the window position changed.
    ///     Reports the new position of the window.
    /// </summary>
    public event EventHandler<Point>? WindowPositionChanged;

    /// <summary>
    ///     Raised when the window size changed.
    ///     Reports the new size of the window.
    /// </summary>
    public event EventHandler<Size>? WindowSizeChanged;

    /**
     * Entry method for Thread.
     * Registers window event hooks and executes the message loop.
     */
    private void Run()
    {
        _threadId = PInvoke.GetCurrentThreadId();

        var hWnd = PInvoke.FindWindow(WindowClass, WindowName);
        if (hWnd != HWND.Null)
        {
            _currentWindowHWnd = hWnd;
            WindowFound?.Invoke(this, hWnd);
        }
        else
        {
            StartWaitForNewWindow();
        }

        // safe-guard: handle any actions enqueued before the thread was started
        ProcessActionQueue();

        // this thread needs a message loop
        // see: https://stackoverflow.com/a/2223270/4161937
        while (PInvoke.GetMessage(out var msg, HWND.Null, 0, 0))
        {
            if (msg.message == WmInvokeAction)
            {
                ProcessActionQueue();
            }

            PInvoke.TranslateMessage(in msg);
            PInvoke.DispatchMessage(in msg);
        }
    }

    private void ProcessActionQueue()
    {
        while (_actionQueue.TryDequeue(out var action))
        {
            try
            {
                _logger.LogDebug("Handling queued action");
                action();
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                _logger.LogError(ex, "Error in dispatched action");
            }
        }
    }

    private void Invoke(Action action)
    {
        _actionQueue.Enqueue(action);

        if (_thread == null)
        {
            return;
        }

        PInvoke.PostThreadMessage(_threadId, WmInvokeAction, UIntPtr.Zero, IntPtr.Zero);
    }

    private static void RegisterWinEventHook(
        uint eventMin,
        uint eventMax,
        // SafeHandle hmodWinEventProc,
        WINEVENTPROC pfnWinEventProc,
        uint idProcess,
        uint idThread,
        uint dwFlags
    )
    {
        var eventHookHandle = PInvoke.SetWinEventHook(
            eventMin,
            eventMax,
            HMODULE.Null,
            WinEventDelegate,
            idProcess,
            idThread,
            dwFlags
        );

        if (eventHookHandle == HWINEVENTHOOK.Null)
        {
            var reason = Marshal.GetLastPInvokeErrorMessage();
            // throw new NativeCallException($"Failed to set WinEventHook: {reason}");
            Console.Write("Failed to set WinEventHook for event range {0}-{1}: {2}", eventMin, eventMax, reason);
        }

        _registeredHooksDictionary.Add(eventHookHandle, pfnWinEventProc);
        ThreadMap.Add(eventHookHandle, Thread.CurrentThread);
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
            var code = Marshal.GetLastWin32Error();
            // throw new NativeCallException($"Failed to set WinEventHook: {reason}");
            // Logger.LogWarning("Failed to unhook win event hook: {hWinEventHook}", hWinEventHook);
            Console.WriteLine("Failed to unhook win event hook: {0} - {1}", hWinEventHook, code);
            Console.WriteLine(
                "Expected Thread: {0} - Actual: {1} - Equal: {2}",
                ThreadMap[hWinEventHook].ManagedThreadId,
                Environment.CurrentManagedThreadId,
                ThreadMap[hWinEventHook] == Thread.CurrentThread
            );
        }

        _registeredHooksDictionary.Remove(hWinEventHook);
        ThreadMap.Remove(hWinEventHook);
    }


    private Size GetWindowSize()
    {
        PInvoke.GetClientRect(_currentWindowHWnd, out var rect);
        var scale = GetDpiScaleFactor(_currentWindowHWnd);

        return new Size((int)(rect.Width / scale), (int)(rect.Height / scale));
    }

    private Point GetWindowPosition()
    {
        var point = new Point(0, 0);
        var success = PInvoke.ClientToScreen(_currentWindowHWnd, ref point);
        if (success)
        {
            var scaleFactor = GetDpiScaleFactor(_currentWindowHWnd);
            point.X = (int)(point.X / scaleFactor);
            point.Y = (int)(point.Y / scaleFactor);
            return point;
        }

        _logger.LogWarning("Failed to get window position");
        return new Point(0, 0); // return value might be zero
    }

    private static double GetDpiScaleFactor(HWND hWnd)
    {
        var monitor = PInvoke.MonitorFromWindow(hWnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        PInvoke.GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out _);
        return dpiX / DefaultDpi;
    }

    private bool IsWindowFocussed()
    {
        var activeWindowHWnd = PInvoke.GetForegroundWindow();

        return activeWindowHWnd == _currentWindowHWnd && activeWindowHWnd != HWND.Null;
    }

    private void EmitInitialState()
    {
        _logger.LogDebug("Emitting initial window state");

        var windowSize = GetWindowSize();
        CurrentWindowSize = windowSize;
        WindowSizeChanged?.Invoke(this, windowSize);

        var windowPosition = GetWindowPosition();
        CurrentWindowPosition = windowPosition;
        WindowPositionChanged?.Invoke(this, windowPosition);

        var windowFocussed = IsWindowFocussed();
        WindowFocusChanged?.Invoke(this, windowFocussed);
    }

    private void StartWindowStateTracking()
    {
        _logger.LogDebug("Starting window state tracking");

        _currentWindowThreadId = PInvoke.GetWindowThreadProcessId(_currentWindowHWnd, out _currentWindowProcessId);

        RegisterWinEventHook(
            PInvoke.EVENT_OBJECT_LOCATIONCHANGE,
            PInvoke.EVENT_OBJECT_LOCATIONCHANGE,
            Handler_WindowMovedOrResized,
            _currentWindowProcessId,
            _currentWindowThreadId,
            PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS
        );

        RegisterWinEventHook(
            PInvoke.EVENT_SYSTEM_MINIMIZESTART,
            PInvoke.EVENT_SYSTEM_MINIMIZESTART,
            Handler_WindowMinimized,
            _currentWindowProcessId,
            _currentWindowThreadId,
            PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS
        );

        RegisterWinEventHook(
            PInvoke.EVENT_SYSTEM_MINIMIZEEND,
            PInvoke.EVENT_SYSTEM_MINIMIZEEND,
            Handler_WindowRestored,
            _currentWindowProcessId,
            _currentWindowThreadId,
            PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS
        );

        RegisterWinEventHook(
            PInvoke.EVENT_OBJECT_FOCUS,
            PInvoke.EVENT_OBJECT_FOCUS,
            Handler_WindowFocused,
            0, // not needed, we need to know if our window has been unfocused
            0, // not needed, we need to know if our window has been unfocused
            PInvoke.WINEVENT_OUTOFCONTEXT // | PInvoke.WINEVENT_SKIPOWNPROCESS
        );
    }

    private void StopWindowStateTracking()
    {
        RemoveAllRegisteredWinEventHooks();

        // reset window handle
        _currentWindowHWnd = default;
    }

    private void StartWaitForNewWindow()
    {
        _logger.LogDebug("Starting wait for new window");

        // start waiting for new window
        RegisterWinEventHook(
            PInvoke.EVENT_OBJECT_CREATE,
            PInvoke.EVENT_OBJECT_CREATE,
            Handler_WindowCreated,
            0,
            0,
            PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS
        );
    }

    private void RemoveAllRegisteredWinEventHooks()
    {
        _logger.LogDebug("Removing all registered win event hooks");

        // unhook all current event listeners
        foreach (var registeredHWinEventHook in _registeredHooksDictionary.Keys)
        {
            UnhookWinEvent(registeredHWinEventHook);
            _registeredHooksDictionary.Remove(registeredHWinEventHook);
        }

        // just to be safe (should, ideally, be completely redundant and a waste of a re-allocation)
        _registeredHooksDictionary = new Dictionary<HWINEVENTHOOK, WINEVENTPROC>();
    }

    private void OnWindowFound(object? sender, HWND hWnd)
    {
        _logger.LogDebug("Window found");

        // emit initial state
        EmitInitialState();
        StartWindowStateTracking();
        StartProcessExitWatcher();
    }

    private void OnProcessExited(object? sender, EventArgs eventArgs)
    {
        _logger.LogDebug("Game process exited");
        StopWindowStateTracking();

        if (!_userPreferencesProvider.CurrentPreferences.TerminateOnGameExit)
        {
            StartWaitForNewWindow();
            return;
        }

        _logger.LogDebug("TerminateOnGameExit enabled - shutting down");
        _applicationLifetime.StopApplication();
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
            _logger.LogWarning(
                "Received window created event but window hWnd is 0: {HWinEventHook}",
                hWinEventHook
            );
            return;
        }

        var windowClass = PInvoke.GetClassName(hWnd);
        var windowTitle = PInvoke.GetWindowText(hWnd);
        var windowProcessName = PInvoke.GetWindowProcessName(hWnd);
        var isTopLevelWindow = PInvoke.IsTopLevelWindow(hWnd);
        var isStarCitizen = windowProcessName?.EndsWith(Constants.GameExecutableName, StringComparison.InvariantCulture) ?? false;

        if (!isStarCitizen)
        {
            return;
        }

        _logger.LogDebug(
            "New Window created - IsTopLevelWindow: {IsTopLevelWindow} - Class: {WindowClass} - Title: {WindowTitle} - IsStarCitizen: {IsStarCitizen}",
            isTopLevelWindow,
            windowClass,
            windowTitle,
            isStarCitizen
        );


        if (windowClass != WindowClass)
        {
            return;
        }

        if (windowTitle != WindowName.Trim())
        {
            return;
        }

        if (!isTopLevelWindow)
        {
            _logger.LogDebug("Window found but it's not a top-level window! WTF");
        }

        // update current window handle - other functions implicitly rely on this!
        _currentWindowHWnd = hWnd;

        // stop listening for WindowCreated events
        UnhookWinEvent(hWinEventHook);

        WindowFound?.Invoke(this, hWnd);
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

        CurrentWindowPosition = GetWindowPosition();
        CurrentWindowSize = GetWindowSize();

        WindowPositionChanged?.Invoke(this, CurrentWindowPosition);
        WindowSizeChanged?.Invoke(this, CurrentWindowSize);
    }

    private void Handler_WindowMinimized(
        HWINEVENTHOOK hWinEventHook,
        uint @event,
        HWND hWnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime
    )
    {
        if (hWnd == HWND.Null)
        {
            return;
        }

        WindowMinimized?.Invoke(this, EventArgs.Empty);
    }

    private void Handler_WindowRestored(
        HWINEVENTHOOK hWinEventHook,
        uint @event,
        HWND hWnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime
    )
    {
        if (hWnd == HWND.Null)
        {
            return;
        }

        WindowRestored?.Invoke(this, EventArgs.Empty);
    }

    private void StartProcessExitWatcher()
    {
        _processExitWatcherCts = new CancellationTokenSource();

        ProcessExitWatcher
            .WaitForProcessExitAsync(_currentWindowProcessId, _processExitWatcherCts.Token)
            .ContinueWith(task =>
                {
                    if (task.IsCanceled)
                    {
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        if (task.Exception is not null)
                        {
                            _logger.LogWarning(task.Exception, "");
                        }

                        Console.WriteLine("Process exit watcher encountered an error: " + task.Exception);
                        return;
                    }

                    // dispatch to WindowTracker worker thread
                    // otherwise we have a lot of fun :)))
                    // => All Native InterOp needs to stick to the same thread
                    Invoke(() => ProcessExited?.Invoke(this, EventArgs.Empty));
                }
            );
    }

    private void StopProcessExitWatcher()
        => _processExitWatcherCts.Cancel();

    public bool IsWindowFocused()
    {
        var hWnd = PInvoke.GetForegroundWindow();
        var isFocused = _currentWindowHWnd != HWND.Null && hWnd == _currentWindowHWnd;

#if DEBUG
        var windowTitle = PInvoke.GetWindowText(hWnd);
        // allows for convenient debugging
        // this way the DevTools window counts as the window being focused
        if (windowTitle != null)
        {
            isFocused |= Debugger.IsAttached && windowTitle.StartsWith("DevTools", StringComparison.InvariantCulture);
        }
#endif

        return isFocused;
    }

    private void Handler_WindowFocused(
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
        // if (hWnd == HWND.Null) return;

        // var isFocused = hWnd == _currentWindowHWnd;
        // sometimes there is an eroneous detected focus change
        // if the above check is used, the below check works 100% of the time
        var currentForegroundWindowHWnd = PInvoke.GetForegroundWindow();
        var isFocused = currentForegroundWindowHWnd == _currentWindowHWnd;

#if DEBUG && !DEBUG
        var windowTitle = PInvoke.GetWindowText(hWnd);
        // allows for convenient debugging
        // this way the DevTools window counts as the window being focused
        isFocused |= Debugger.IsAttached && (windowTitle?.StartsWith("DevTools", StringComparison.InvariantCulture) ?? false);
#endif

        _logger.LogDebug(
            "Window focus changed: {IsFocused} => Current hWnd: {HWnd} - Focused hWnd: {CurrentForegroundWindowHWnd}",
            isFocused,
            (IntPtr)_currentWindowHWnd,
            (IntPtr)currentForegroundWindowHWnd
        );
        WindowFocusChanged?.Invoke(this, isFocused);
    }
}
