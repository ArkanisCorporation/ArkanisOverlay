using static Windows.Win32.PInvoke;

namespace Arkanis.Overlay.Host.Desktop.UI.Windows;

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Common;
using Domain.Abstractions.Services;
using Domain.Options;
using global::Windows.Win32.Foundation;
using global::Windows.Win32.UI.WindowsAndMessaging;
using Helpers;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Services.Factories;
using Workers;

/// <summary>
///     Interaction logic for OverlayWindow.xaml
/// </summary>
public sealed partial class OverlayWindow : IDisposable
{
    private readonly BlurHelper _blurHelper;
    private readonly GlobalHotkey _globalHotkey;

    private readonly ILogger _logger;
    private readonly IUserPreferencesProvider _preferencesProvider;
    private readonly WindowFactory _windowFactory;
    private readonly WindowTracker _windowTracker;

    private HWND _currentWindowHWnd = HWND.Null;
    private HWND _currentGameHWnd = HWND.Null;

    public OverlayWindow(
        ILogger<OverlayWindow> logger,
        IUserPreferencesProvider preferencesProvider,
        WindowTracker windowTracker,
        GlobalHotkey globalHotkey,
        BlurHelper blurHelper,
        WindowFactory windowFactory
    )
    {
        Instance = this;

        _logger = logger;
        _preferencesProvider = preferencesProvider;
        _windowTracker = windowTracker;
        _globalHotkey = globalHotkey;
        _blurHelper = blurHelper;
        _windowFactory = windowFactory;

        SetupWorkerEventListeners();
        InitializeComponent();

        // fallback in case the window was already found
        _currentWindowHWnd = _windowTracker.CurrentWindowHWnd;

        Height = _windowTracker.CurrentWindowSize.Height;
        Width = _windowTracker.CurrentWindowSize.Width;

        Top = _windowTracker.CurrentWindowPosition.Y;
        Left = _windowTracker.CurrentWindowPosition.X;

        BlazorWebView.BlazorWebViewInitializing += BlazorWebView_Initializing;
        _preferencesProvider.ApplyPreferences += ApplyUserPreferences;
    }


    public static OverlayWindow? Instance { get; private set; }

    private void ApplyUserPreferences(object? sender, UserPreferences newPreferences)
    {
        // prevent attempting to set blur when window is not visible
        // which would lead to a crash
        if (!IsVisible)
        {
            return;
        }

        Dispatcher.Invoke(() => _blurHelper.SetBlurEnabled(newPreferences.BlurBackground));
    }

    private void ShowOverlay()
    {
        ForceFocusSelf();

        // only works when window is visible
        _blurHelper.SetBlurEnabled(_preferencesProvider.CurrentPreferences.BlurBackground);

        var result = Activate();
        _logger.LogDebug("Overlay: Activate Window: {Result}", result);

        BlazorWebView.WebView.Focus();
    }

    private void ForceFocus(HWND targetHWnd)
    {
        Dispatcher.Invoke(() =>
            {
                _logger.LogDebug("Overlay: ForceFocus: {TargetHWnd}", targetHWnd);
                var foregroundWindowHWnd = GetForegroundWindow();
                if (foregroundWindowHWnd == targetHWnd)
                {
                    // already focused
                    return;
                }

                var currentThreadId = GetCurrentThreadId();

                var sourceThreadId = IsActive ? currentThreadId : GetWindowThreadProcessId(foregroundWindowHWnd, out _);
                var targetThreadId = targetHWnd == _currentWindowHWnd ? currentThreadId : GetWindowThreadProcessId(targetHWnd, out _);

                var threadsToAttach = new List<uint>();
                if (sourceThreadId != currentThreadId)
                {
                    threadsToAttach.Add(sourceThreadId);
                }

                if (targetThreadId != currentThreadId)
                {
                    threadsToAttach.Add(targetThreadId);
                }

                SetThreadInputAttached(threadsToAttach, currentThreadId, true);

                var success = BringWindowToTop(targetHWnd);
                if (!success)
                {
                    var code = Marshal.GetLastWin32Error();
                    var reason = Marshal.GetLastPInvokeErrorMessage();
                    _logger.LogWarning("Failed to bring window to top: {Code} - {Reason}", code, reason);
                }

                SetThreadInputAttached(threadsToAttach, currentThreadId, false);

            }
        );
        return;

        void SetThreadInputAttached(List<uint> threadsToAttach, uint threadToAttachTo, bool attached)
        {
            foreach (var threadId in threadsToAttach)
            {
                _logger.LogDebug("Overlay: SetThreadInputAttached: {ThreadId} - {ThreadToAttachTo} - {Attached}", threadId, threadToAttachTo, attached);
                var success = AttachThreadInput(threadId, threadToAttachTo, attached);
                if (success)
                {
                    continue;
                }

                var code = Marshal.GetLastWin32Error();
                var reason = Marshal.GetLastPInvokeErrorMessage();
                _logger.LogWarning("Failed to attach thread input: {Code} - {Reason}", code, reason);
            }
        }
    }

    private void ForceFocusSelf()
    {
        ForceFocus(_currentWindowHWnd);
        Show();
    }

    private void ForceFocusGame()
        => ForceFocus(_currentGameHWnd);


    private void SetupWorkerEventListeners()
    {
        _windowTracker.WindowFound +=
            (_, hWnd) => Dispatcher.Invoke(() => { _currentGameHWnd = hWnd; });
        _windowTracker.ProcessExited +=
            (_, _) => Dispatcher.Invoke(() =>
                {
                    _currentGameHWnd = HWND.Null;
                    HideOverlay();
                }
            );
        _windowTracker.WindowPositionChanged += (_, position) => Dispatcher.Invoke(() =>
            {
                _logger.LogDebug("Overlay: WindowPositionChanged: {Position}", position.ToString());
                Top = position.Y;
                Left = position.X;
            }
        );
        _windowTracker.WindowSizeChanged += (_, size) => Dispatcher.Invoke(() =>
            {
                _logger.LogDebug("Overlay: WindowSizeChanged: {Size}", size.ToString());
                Width = size.Width;
                Height = size.Height;
            }
        );

        _windowTracker.WindowFocusChanged += (_, isFocused) => Dispatcher.Invoke(() =>
            {
                _logger.LogDebug("Overlay: WindowFocusChanged: {IsFocused}", isFocused);
                if (Visibility != Visibility.Visible) { return; }

                Topmost = isFocused;
            }
        );

        _globalHotkey.ConfiguredHotKeyPressed += (_, _) => Dispatcher.Invoke(() =>
            {
                _logger.LogDebug("Overlay: HotKeyPressed");
                if (Visibility == Visibility.Visible)
                {
                    HideOverlay();
                    return;
                }

                if (!_windowTracker.IsWindowFocused())
                {
                    return;
                }

                ShowOverlay();
            }
        );
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
#if WITH_KEYBOARD_DEBUGGING
        _logger.LogTrace("Overlay: KeyDown: {Key}", e.Key);
#endif

        base.OnKeyDown(e);
    }

    // private void Handle_UrlLoading(object sender, UrlLoadingEventArgs urlLoadingEventArgs)
    // {
    //     if (urlLoadingEventArgs.Url.Host != "0.0.0.0")
    //     {
    //         urlLoadingEventArgs.UrlLoadingStrategy =
    //             UrlLoadingStrategy.OpenInWebView;
    //     }
    // }

    private void SetExtendedWindowStyle()
    {
        var wndHelper = new WindowInteropHelper(this);

        var exStyle = GetWindowLong((HWND)wndHelper.Handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        exStyle |= (int)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW;
        _ = SetWindowLong((HWND)wndHelper.Handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, exStyle);
    }

    private void BlazorWebView_OnLoaded(object? sender, RoutedEventArgs e)
    {
        SetExtendedWindowStyle();

        BlazorWebView.WebView.DefaultBackgroundColor = Color.Transparent;
        BlazorWebView.WebView.NavigationCompleted += WebView_Loaded;
        BlazorWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
        Visibility = Visibility.Collapsed;
    }

    private void BlazorWebView_Initializing(object? sender, BlazorWebViewInitializingEventArgs e)
        => e.UserDataFolder = Path.Join(ApplicationConstants.ApplicationDataDirectory.FullName, "WebView");

    private void WebView_Loaded(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        // If we are running in a development/debugger mode, open dev tools to help out
        if (Debugger.IsAttached)
        {
            BlazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
        }

        BlazorWebView.Focus();
    }

    private void CoreWebView_Loaded(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        // BlazorWebView.WebView.CoreWebView2.SetVirtualHostNameToFolderMapping("resources.internal", "Resources", CoreWebView2HostResourceAccessKind.Allow);
        BlazorWebView.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        BlazorWebView.WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
        BlazorWebView.WebView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
    }

    private void HideOverlay()
        => Collapse();

    /// <summary>
    ///     Interface to hide the overlay from JS.
    ///     Collapses the window. This will hide the window and give focus back to Star Citizen.
    /// </summary>
    public void Collapse()
    {
        Visibility = Visibility.Collapsed;

        // we switch focus back to Star Citizen because
        // otherwise the previously active window will
        // receive focus instead for some reason
        if (_currentGameHWnd == HWND.Null)
        {
            return;
        }

        ForceFocusGame();
    }

    private void OnPreferenceCommand(object sender, RoutedEventArgs e)
        => _windowFactory.CreateWindow<PreferencesWindow>().ShowDialog();

    private void OnAboutCommand(object sender, RoutedEventArgs e)
        => _windowFactory.CreateWindow<AboutWindow>().ShowDialog();

    private void OnExitCommand(object sender, RoutedEventArgs e)
        => Application.Current.Shutdown();

    public void Exit()
        => Dispatcher.Invoke(() => OnExitCommand(this, new RoutedEventArgs()));

    public void Dispose()
    {
        _globalHotkey.Dispose();
        _windowTracker.Dispose();
        if (BlazorWebView is IDisposable blazorWebViewDisposable)
        {
            blazorWebViewDisposable.Dispose();
        }
        else if (BlazorWebView != null)
        {
            _ = BlazorWebView.DisposeAsync().AsTask();
        }
    }

    private void OverlayWindow_OnLoaded(object sender, RoutedEventArgs e)
        => _currentWindowHWnd = (HWND)new WindowInteropHelper(this).Handle;
}
