namespace Arkanis.Overlay.Host.Desktop.UI.Windows;

using System.Diagnostics;
using System.Windows;
using global::Windows.Win32.UI.WindowsAndMessaging;
using Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Workers;

public partial class HudWindow
{
    private readonly ILogger<HudWindow> _logger;
    private readonly WindowTracker _windowTracker;

    public HudWindow(
        ILogger<HudWindow> logger,
        WindowTracker windowTracker
    )
    {
        _logger = logger;
        _windowTracker = windowTracker;

        InitializeComponent();

        IsVisibleChanged += (_, _) => _logger.LogDebug("HudWindow: VisibilityChanged: {IsVisible}", IsVisible);

        _windowTracker.WindowSizeChanged += (_, size) => Dispatcher.Invoke(() =>
                {
                    // _logger.LogDebug("HudWindow: WindowSize: {Width}x{Height}", size.Width, size.Height);
                    MaxWidth = MinWidth = size.Width;
                    MaxHeight = MinHeight = size.Height;
                }
            );

        _windowTracker.WindowPositionChanged += (_, position) => Dispatcher.Invoke(() =>
                {
                    // _logger.LogDebug("HudWindow: WindowPosition: {X},{Y}", position.X, position.Y);
                    Left = position.X;
                    Top = position.Y;
                }
            );

        _windowTracker.WindowFocusChanged += (_, focused) => Dispatcher.Invoke(() => Visibility = focused ? Visibility.Visible : Visibility.Collapsed);

        var visibilityBeforeWindowSizeOrPositionChange = Visibility;
        _windowTracker.WindowSizeOrPositionChangeStart += (_, _) => Dispatcher.Invoke(() =>
                {
                    _logger.LogDebug("HudWindow: WindowSizeOrPositionChanging");
                    visibilityBeforeWindowSizeOrPositionChange = Visibility;
                    Visibility = Visibility.Collapsed;
                }
            );

        _windowTracker.WindowSizeOrPositionChangeEnd += (_, _) => Dispatcher.Invoke(() =>
                    // _logger.LogDebug("HudWindow: WindowSizeOrPositionChanged");
                    Visibility = visibilityBeforeWindowSizeOrPositionChange);

        LocationChanged += (_, __) => NudgePopup();
    }

    private void OnWindowInitialized(object sender, EventArgs e)
    {
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        WindowUtils.SetExtendedStyle(
            this,
            WINDOW_EX_STYLE.WS_EX_TRANSPARENT
            | WINDOW_EX_STYLE.WS_EX_NOACTIVATE
        //|  WINDOW_EX_STYLE.WS_EX_LAYERED
        );

        Top = _windowTracker.CurrentWindowPosition.Y;
        Left = _windowTracker.CurrentWindowPosition.X;

        MaxWidth = MinWidth = _windowTracker.CurrentWindowSize.Width;
        MaxHeight = MinHeight = _windowTracker.CurrentWindowSize.Height;
    }

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        BlazorWebView.WebView.DefaultBackgroundColor = Color.Transparent;
        BlazorWebView.WebView.NavigationCompleted += WebView_Loaded;
        BlazorWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
        Visibility = Visibility.Collapsed; // workaround to prevent the window from being shown on startup
        // Window.Show() sets Visibility to Visible
        // Window.Show() also causes the window contents to load, so it's required
        // This is the last step in the initialization / loading process, so we can collapse the window right after
    }

    private void CoreWebView_Loaded(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        BlazorWebView.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        BlazorWebView.WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
        BlazorWebView.WebView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
    }

    private void WebView_Loaded(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        // If we are running in a development/debugger mode, open dev tools to help out
        if (Debugger.IsAttached)
        {
            BlazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
        }
    }

    private void NudgePopup()
    {
        if (DebugPanel?.IsOpen != true)
        {
            return;
        }

        // Toggle offset by a pixel to force WPF to recompute placement
        var o = DebugPanel.HorizontalOffset;
        DebugPanel.HorizontalOffset = o + 1;
        DebugPanel.HorizontalOffset = o;
    }
}
