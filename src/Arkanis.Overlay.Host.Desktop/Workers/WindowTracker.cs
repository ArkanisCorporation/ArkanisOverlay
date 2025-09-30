namespace Arkanis.Overlay.Host.Desktop.Workers;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.HiDpi;
using Domain.Abstractions.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Timer = System.Threading.Timer;

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
    public Size CurrentWindowSize { get; private set; }
    public Point CurrentWindowPosition { get; private set; }
    public bool IsWindowFocused { get; private set; }

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

    /// <summary>
    ///     Raised when the window size or position change has started.
    ///     Use this event to hide the Overlay while the window is being moved or resized.
    /// </summary>
    public event EventHandler? WindowSizeOrPositionChangeStart;

    /// <summary>
    ///     Raised when the window size or position change has ended.
    ///     Use this event to restore the Overlay after the window has finished being moved or resized.
    /// </summary>
    public event EventHandler? WindowSizeOrPositionChangeEnd;

    public WindowTracker(
        IHostApplicationLifetime applicationLifetime,
        IUserPreferencesProvider userPreferencesProvider,
        ILogger<WindowTracker> logger
    )
    {
        _applicationLifetime = applicationLifetime;
        _userPreferencesProvider = userPreferencesProvider;
        _windowSizeAndPositionDebounceTimer = new Timer(OnDebounceTimer_UpdateWindowSizeAndPosition);
        _logger = logger;

        ProcessExited += OnProcessExited;
        WindowFound += OnWindowFound;
    }

    /// <summary>
    ///     Raised when the Star Citizen game window was found.
    /// </summary>
    internal event EventHandler<HWND>? WindowFound;

    private static Dictionary<HWINEVENTHOOK, WINEVENTPROC> _registeredHooksDictionary = new();
    private static readonly Dictionary<HWINEVENTHOOK, Thread> ThreadMap = new();

    private const uint WmInvokeAction = PInvoke.WM_USER + 100;
    private const double DefaultDpi = 96.0; // This is official and fixed by Windows logic

    private const string WindowClass = Constants.WindowClass;
    private const string WindowName = Constants.WindowName;

    private const uint WindowSizeAndPositionDebounceMs = 120;

    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IUserPreferencesProvider _userPreferencesProvider;

    private readonly Timer _windowSizeAndPositionDebounceTimer;

    private readonly ConcurrentQueue<Action> _actionQueue = new();
    private CancellationTokenSource _processExitWatcherCts = new();

    private HWND _currentWindowHWnd;
    private uint _currentWindowProcessId;
    private uint _currentWindowThreadId;
    private bool _isWindowSizeAndPositionDebounceTimerRunning;

    /// <summary>
    ///     The self-launched thread this class runs on.
    ///     This is needed to be able to stop the thread.
    /// </summary>
    private Thread? _thread;

    private uint _threadId;


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

    //? For future refactoring to improve developer QOL / interface quality & usability
    // /// <summary>
    // ///
    // /// </summary>
    // /// <remarks>
    // ///     By default, we do not know the current window state, so we have "lost" track of it
    // /// </remarks>
    // private ExtendedWindowState _currentWindowState = ExtendedWindowState.Lost;

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

        if (_thread == null) { return; }

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
            Log.Fatal("Failed to unhook win event hook: {0} - Code: {1}", hWinEventHook, code);
            Log.Fatal(
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

    private void EmitInitialState()
    {
        _logger.LogDebug("Emitting initial window state");

        UpdateWindowSizeAndPosition();

        IsWindowFocused = GetWindowFocus();
        WindowFocusChanged?.Invoke(this, IsWindowFocused);
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
            PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS
            // PInvoke.WINEVENT_OUTOFCONTEXT
        );

        RegisterWinEventHook(
            PInvoke.EVENT_OBJECT_REORDER,
            PInvoke.EVENT_OBJECT_REORDER,
            Handler_ObjectsReordered,
            _currentWindowProcessId,
            _currentWindowThreadId,
            PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS
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

        IsWindowFocused = false;
        DispatchFast(() => WindowFocusChanged?.Invoke(this, IsWindowFocused));

        if (!_userPreferencesProvider.CurrentPreferences.TerminateOnGameExit)
        {
            StartWaitForNewWindow();
            return;
        }

        _logger.LogDebug("TerminateOnGameExit enabled - shutting down");
        _applicationLifetime.StopApplication();
    }

    private void StartProcessExitWatcher()
    {
        _processExitWatcherCts.Cancel();
        _processExitWatcherCts.Dispose();
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
                            _logger.LogWarning(task.Exception, "Process exit watcher encountered an error");
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
    {
        try
        {
            _processExitWatcherCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private bool GetWindowFocus(HWND? targetHWnd = null)
    {
        var hWnd = targetHWnd ?? PInvoke.GetForegroundWindow();
        var windowTitle = PInvoke.GetWindowText(hWnd);

        PInvoke.GetWindowThreadProcessId(hWnd, out var focusedWindowProcessId);
        var processName = PInvoke.GetWindowProcessName(hWnd);

        var isFocused =
            (_currentWindowHWnd != HWND.Null && hWnd == _currentWindowHWnd)
            || (_currentWindowProcessId == focusedWindowProcessId);

#if DEBUG
        // allows for convenient debugging
        // this way the DevTools window counts as the window being focused
        if (windowTitle != null)
        {
            isFocused |= Debugger.IsAttached && windowTitle.StartsWith("DevTool", StringComparison.InvariantCulture);
        }
#endif

        _logger.LogDebug(
            "GetWindowFocus: HWnd: {HWnd} - IsFocused: {IsFocused} - Title: {Title} - ProcessName: {ProcessName}",
            hWnd,
            isFocused,
            windowTitle,
            processName
        );

        return isFocused;
    }

    private void OnDebounceTimer_UpdateWindowSizeAndPosition(object? state)
    {
        _logger.LogDebug("Debounce timer triggered, updating window size and position");
        // reset flag
        _isWindowSizeAndPositionDebounceTimerRunning = false;

        DispatchFast(() =>
            {
                UpdateWindowSizeAndPosition();

                WindowSizeOrPositionChangeEnd?.Invoke(null, EventArgs.Empty);
            }
        );
    }

    private void UpdateWindowSizeAndPosition()
    {
        var windowPosition = GetWindowPosition();
        var windowSize = GetWindowSize();

        //? this is okay because it does by-value comparison internally
        if (windowPosition != CurrentWindowPosition)
        {
            CurrentWindowPosition = windowPosition;
            WindowPositionChanged?.Invoke(null, windowPosition);
        }

        //? this is okay because it does by-value comparison internally
        if (windowSize != CurrentWindowSize)
        {
            CurrentWindowSize = windowSize;
            WindowSizeChanged?.Invoke(null, windowSize);
        }
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
            DispatchFast(() =>
                _logger.LogWarning(
                    "Received window created event but window hWnd is 0: {HWinEventHook}",
                    hWinEventHook
                )
            );
            return;
        }

        var windowClass = PInvoke.GetClassName(hWnd);
        var windowTitle = PInvoke.GetWindowText(hWnd);
        var windowProcessName = PInvoke.GetWindowProcessName(hWnd);
        var isTopLevelWindow = PInvoke.IsTopLevelWindow(hWnd);

        var isStarCitizen = windowProcessName?
                                .EndsWith(Constants.GameExecutableName, StringComparison.InvariantCulture)
                            ?? false;

        if (!isStarCitizen) { return; }

        DispatchFast(() =>
            _logger.LogDebug(
                "New Window created - IsTopLevelWindow: {IsTopLevelWindow} - Class: {WindowClass} - Title: {WindowTitle} - IsStarCitizen: {IsStarCitizen}",
                isTopLevelWindow,
                windowClass,
                windowTitle,
                isStarCitizen
            )
        );

        if (windowClass != WindowClass) { return; }

        if (windowTitle != WindowName.Trim()) { return; }

        if (!isTopLevelWindow)
        {
            DispatchFast(() =>
                _logger.LogDebug("Window found but it's not a top-level window! WTF")
            );
            return;
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
        var startTicks = HookTiming.NowTicks();

        // safety precaution
        if (hWnd == HWND.Null)
        {
            return;
        }

        if (!_isWindowSizeAndPositionDebounceTimerRunning)
        {
            DispatchFast(() => WindowSizeOrPositionChangeStart?.Invoke(null, EventArgs.Empty));
        }

        if (_windowSizeAndPositionDebounceTimer.Change(WindowSizeAndPositionDebounceMs, Timeout.Infinite))
        {
            _isWindowSizeAndPositionDebounceTimerRunning = true;
            DispatchFast(() => _logger.LogDebug(
                    "WindowMovedOrResized: Updated debounce timer to run in {DebounceMs}ms",
                    WindowSizeAndPositionDebounceMs
                )
            );
        }
        else
        {
            DispatchFast(() => _logger.LogDebug("WindowMovedOrResized: Failed to change debounce timer"));
        }

        DispatchFast(() => _logger.LogDebug(
                "WindowMovedOrResized: Elapsed: {ElapsedUs} us",
                HookTiming.ElapsedUs(startTicks)
            )
        );
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

        DispatchFast(() => WindowMinimized?.Invoke(null, EventArgs.Empty));
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

        DispatchFast(() => WindowRestored?.Invoke(null, EventArgs.Empty));
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
        var startTicks = HookTiming.NowTicks();

        var isFocused = false;
        var windowClass = PInvoke.GetClassName(hWnd);
        var isGhostWindow = windowClass == Constants.GhostWindowClassName;

        // only dispatch if the state has changed
        if (!isGhostWindow)
        {
            // might work better for launch focus change detection
            // isFocused = GetWindowFocus(hWnd);

            // auto-detects currently focused window with GetForegroundWindow() internally
            // fixes Alt + Tab bug
            isFocused = GetWindowFocus();

            IsWindowFocused = isFocused;
            // WindowFocusChanged?.Invoke(null, isFocused);
            DispatchFast(() => WindowFocusChanged?.Invoke(null, isFocused));
        }

        DispatchFast(() =>
            _logger.LogDebug(
                "WindowFocused: {IsFocused} - IsGhostWindow: {IsGhostWindow} - Elapsed: {ElapsedUs} us",
                isFocused,
                isGhostWindow,
                HookTiming.ElapsedUs(startTicks)
            )
        );
    }

    private void Handler_ObjectsReordered(
        HWINEVENTHOOK hWinEventHook,
        uint @event,
        HWND hWnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime
    )
        => DispatchFast(UpdateWindowSizeAndPosition);

    private static void DispatchFast(Action action)
        => ThreadPool.UnsafeQueueUserWorkItem(
            delegate
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Delegate actionDelegate = action;

                    DispatchFast(() => Log.Error(
                            ex,
                            "DispatchFast: Unhandled Exception in dispatched action - Action: {Action}.{Method}",
                            actionDelegate.Method.ReflectedType?.FullName ?? "Unknown",
                            actionDelegate.Method.Name
                        )
                    );
                }
            },
            null
        );

    internal static class HookTiming
    {
        public static readonly double TicksToUs = 1_000_000.0 / Stopwatch.Frequency;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long NowTicks()
            => Stopwatch.GetTimestamp();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ElapsedUs(long startTicks)
            => (int)((Stopwatch.GetTimestamp() - startTicks) * TicksToUs);
    }
}
