namespace Arkanis.Overlay.Host.Desktop.UI.Windows;

using System.Diagnostics;
using System.Windows;
using Domain.Abstractions.Services;
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

        Top = _windowTracker.CurrentWindowPosition.Y;
        Left = _windowTracker.CurrentWindowPosition.X;

        Height = _windowTracker.CurrentWindowSize.Height;
        Width = _windowTracker.CurrentWindowSize.Width;

        _windowTracker.WindowSizeChanged += (_, size) =>
        {
            Dispatcher.Invoke(() =>
                {
                    // _logger.LogDebug("HudWindow: WindowSize: {Width}x{Height}", size.Width, size.Height);
                    Width = size.Width;
                    Height = size.Height;
                }
            );
        };

        _windowTracker.WindowPositionChanged += (_, position) =>
        {
            Dispatcher.Invoke(() =>
                {
                    // _logger.LogDebug("HudWindow: WindowPosition: {X},{Y}", position.X, position.Y);
                    Left = position.X;
                    Top = position.Y;
                }
            );
        };

        _windowTracker.WindowFocusChanged += (_, focused) =>
        {
            Dispatcher.Invoke(() =>
                {
                    _logger.LogDebug("HudWindow: WindowFocused: {IsFocused}", focused);
                    // Topmost = focused;
                    Visibility = focused ? Visibility.Visible : Visibility.Hidden;
                    _logger.LogDebug("HudWindow: Visibility: {Visibility}", Visibility);
                    _logger.LogDebug("HudWindow: Position: {X},{Y}", Left, Top);
                    _logger.LogDebug("HudWindow: Size: {Width}x{Height}", Width, Height);
                }
            );
        };

        var visibilityBeforeWindowSizeOrPositionChange = Visibility;
        _windowTracker.WindowSizeOrPositionChangeStart += (_, _) =>
        {
            Dispatcher.Invoke(() =>
                {
                    _logger.LogDebug("HudWindow: WindowSizeOrPositionChanging");
                    visibilityBeforeWindowSizeOrPositionChange = Visibility;
                    Visibility = Visibility.Collapsed;
                }
            );
        };

        _windowTracker.WindowSizeOrPositionChangeEnd += (_, _) =>
        {
            Dispatcher.Invoke(() =>
                {
                    // _logger.LogDebug("HudWindow: WindowSizeOrPositionChanged");
                    Visibility = visibilityBeforeWindowSizeOrPositionChange;
                }
            );
        };

        InitializeComponent();
    }

    private void OnWindowInitialized(object sender, EventArgs e)
    {
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
        => WindowUtils.SetExtendedStyle(
            this,
            WINDOW_EX_STYLE.WS_EX_TRANSPARENT
            | WINDOW_EX_STYLE.WS_EX_NOACTIVATE
            //|  WINDOW_EX_STYLE.WS_EX_LAYERED
        );

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        BlazorWebView.WebView.DefaultBackgroundColor = Color.Transparent;
        BlazorWebView.WebView.NavigationCompleted += WebView_Loaded;
        BlazorWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
    }

    private void CoreWebView_Loaded(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        BlazorWebView.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        BlazorWebView.WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
    }

    private void WebView_Loaded(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        // If we are running in a development/debugger mode, open dev tools to help out
        if (Debugger.IsAttached)
        {
            BlazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
        }

        BlazorWebView.Focus();
    }
}
