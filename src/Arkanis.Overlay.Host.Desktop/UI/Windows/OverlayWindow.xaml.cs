namespace Arkanis.Overlay.Host.Desktop.UI.Windows;
using static global::Windows.Win32.PInvoke;

using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Common;
using Common.Options;
using Domain.Abstractions.Services;
using global::Windows.Win32.Foundation;
using global::Windows.Win32.UI.WindowsAndMessaging;
using Helpers;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Services.Factories;
using Workers;
using Color = Color;

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
    private readonly IOverlayEventControls _overlayEventControls;
    private readonly WindowTracker _windowTracker;

    private HWND _currentWindowHWnd = HWND.Null;

    public OverlayWindow(
        ILogger<OverlayWindow> logger,
        IUserPreferencesProvider preferencesProvider,
        WindowTracker windowTracker,
        GlobalHotkey globalHotkey,
        BlurHelper blurHelper,
        WindowFactory windowFactory,
        IOverlayEventControls overlayEventControls
    )
    {
        Instance = this;

        _logger = logger;
        _preferencesProvider = preferencesProvider;
        _windowTracker = windowTracker;
        _globalHotkey = globalHotkey;
        _blurHelper = blurHelper;
        _windowFactory = windowFactory;
        _overlayEventControls = overlayEventControls;

        SetupWorkerEventListeners();
        InitializeComponent();

        MaxWidth = MinWidth = _windowTracker.CurrentWindowSize.Width;
        MaxHeight = MinHeight = _windowTracker.CurrentWindowSize.Height;

        Top = _windowTracker.CurrentWindowPosition.Y;
        Left = _windowTracker.CurrentWindowPosition.X;

        BlazorWebView.BlazorWebViewInitializing += BlazorWebView_Initializing;
        _preferencesProvider.ApplyPreferences += ApplyUserPreferences;

        LocationChanged += (_, __) => NudgePopup();
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
        ForceFocus();

        // only works when window is visible
        _blurHelper.SetBlurEnabled(_preferencesProvider.CurrentPreferences.BlurBackground);

        var result = Activate();
        _logger.LogDebug("Overlay: Activate Window: {Result}", result);

        BlazorWebView.WebView.Focus();
        _overlayEventControls.OnWindowShown();
    }

    private void ForceFocus()
    {
        var mainWindowHandle = (HWND)new WindowInteropHelper(this).Handle;
        var windowThreadProcessId = GetWindowThreadProcessId(GetForegroundWindow(), out _);
        var currentThreadId = GetCurrentThreadId();
        AttachThreadInput(windowThreadProcessId, currentThreadId, true);
        BringWindowToTop(mainWindowHandle);
        Show();
        AttachThreadInput(windowThreadProcessId, currentThreadId, false);
    }


    private void SetupWorkerEventListeners()
    {
        _windowTracker.WindowFound +=
            (_, hWnd) => Dispatcher.Invoke(() => _currentWindowHWnd = hWnd);
        _windowTracker.ProcessExited +=
            (_, _) => Dispatcher.Invoke(() =>
                {
                    _currentWindowHWnd = HWND.Null;
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
                MaxWidth = MinWidth = size.Width;
                MaxHeight = MinHeight = size.Height;
            }
        );

        _windowTracker.WindowFocusChanged += (_, isFocused) => Dispatcher.Invoke(() =>
            {
                _logger.LogDebug("Overlay: WindowFocusChanged: {IsFocused}", isFocused);
                if (isFocused && Visibility == Visibility.Visible)
                {
                    ForceFocus();
                }
            }
        );

        var visibilityBeforeWindowSizeOrPositionChange = Visibility;
        _windowTracker.WindowSizeOrPositionChangeStart += (_, _) => Dispatcher.Invoke(() =>
                {
                    _logger.LogDebug("HudWindow: WindowSizeOrPositionChanging");
                    visibilityBeforeWindowSizeOrPositionChange = Visibility;
                    Visibility = Visibility.Collapsed;
                }
            );

        _windowTracker.WindowSizeOrPositionChangeEnd += (_, _) => Dispatcher.Invoke(() =>
                {
                    _logger.LogDebug("HudWindow: WindowSizeOrPositionChanged");
                    Visibility = visibilityBeforeWindowSizeOrPositionChange;
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

                if (!_windowTracker.IsWindowFocused)
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

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        WindowUtils.SetExtendedStyle(
            this,
            WINDOW_EX_STYLE.WS_EX_TOOLWINDOW
        // | WINDOW_EX_STYLE.WS_EX_LAYERED
        // | WINDOW_EX_STYLE.WS_EX_NOACTIVATE
        // | WINDOW_EX_STYLE.WS_EX_TRANSPARENT
        );

        BlazorWebView.WebView.DefaultBackgroundColor = Color.Transparent;
        BlazorWebView.WebView.NavigationCompleted += WebView_Loaded;
        BlazorWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
        Visibility = Visibility.Collapsed; // workaround to prevent the window from being shown on startup
        // Window.Show() sets Visibility to Visible
        // Window.Show() also causes the window contents to load, so it's required
        // This is the last step in the initialization / loading process, so we can collapse the window right after
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
        if (_currentWindowHWnd == HWND.Null)
        {
            return;
        }

        SetForegroundWindow(_currentWindowHWnd);
    }

    private void NudgePopup()
    {
        if (DebugPanel?.IsOpen != true)
        {
            return;
        }

        // Toggle offset by a pixel to force WPF to recompute placement
        var horizontalOffset = DebugPanel.HorizontalOffset;
        DebugPanel.HorizontalOffset = horizontalOffset + 1;
        DebugPanel.HorizontalOffset = horizontalOffset;
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
}
