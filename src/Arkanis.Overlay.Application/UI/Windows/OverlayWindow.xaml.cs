﻿using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Arkanis.Overlay.Application.Helpers;
using Arkanis.Overlay.Application.Workers;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using static Windows.Win32.PInvoke;
using Index = Arkanis.Overlay.Application.UI.Pages.Overlay.Index;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Arkanis.Overlay.Application.UI.Windows;

/// <summary>
/// Interaction logic for OverlayWindow.xaml
/// </summary>
public partial class OverlayWindow
{
    public static OverlayWindow? Instance { get; private set; }

    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly WindowTracker _windowTracker;
    private readonly GlobalHotkey _globalHotkey;
    private readonly BlurHelper _blurHelper;

    private HWND _currentWindowHWnd = HWND.Null;


    public OverlayWindow(
        ILogger<OverlayWindow> logger, IHostApplicationLifetime hostApplicationLifetime,
        WindowTracker windowTracker, GlobalHotkey globalHotkey,
        BlurHelper blurHelper)
    {
        Instance = this;

        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _windowTracker = windowTracker;
        _globalHotkey = globalHotkey;
        _blurHelper = blurHelper;

        SetupWorkerEventListeners();

        _blurHelper.EnableBlur(this, 1);

        InitializeComponent();
        BlazorWebView.BlazorWebViewInitializing += BlazorWebView_Initializing;
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        _windowTracker.Start();
        _globalHotkey.Start();
    }

    private void HideOverlay()
    {
        Collapse();
    }

    private void ShowOverlay()
    {
        var mainWindowHandle = (HWND)new WindowInteropHelper(this).Handle;
        var windowThreadProcessId = GetWindowThreadProcessId(GetForegroundWindow(), out _);
        var currentThreadId = GetCurrentThreadId();
        AttachThreadInput(windowThreadProcessId, currentThreadId, true);
        BringWindowToTop(mainWindowHandle);
        Show();
        // PInvoke.ShowWindow((HWND)MainWindowHandle, SHOW_WINDOW_CMD.SW_SHOW);
        AttachThreadInput(windowThreadProcessId, currentThreadId, false);

        // only works when window is visible
        _blurHelper.EnableBlur(this, 1);

        var result = Activate();
        _logger.LogDebug("ShowOverlay(): Activate Window: {result}", result);

        BlazorWebView.WebView.Focus();

        // blazorWebView.Focusable = true;
        // result = blazorWebView.Focus();
        // _logger.LogDebug("ShowOverlay(): Focus WebView: {result}", result);

        // Keyboard.Focus(blazorWebView);
        // Mouse.Capture(blazorWebView);

        // Index.SearchBox.FocusAsync();
        Index.SearchBox?.SelectAsync();
        // Index.Instance?.ShowSnackbar();
    }

    private void SetupWorkerEventListeners()
    {
        _windowTracker.WindowFound += (_, hWnd) => Dispatcher.Invoke(() => { _currentWindowHWnd = hWnd; });
        _windowTracker.WindowLost += (_, _) => Dispatcher.Invoke(() => { _currentWindowHWnd = HWND.Null; });
        _windowTracker.WindowPositionChanged += (_, position) => Dispatcher.Invoke(() =>
        {
            _logger.LogDebug("Overlay: WindowPositionChanged: {position}", position.ToString());
            Top = position.Y;
            Left = position.X;
        });
        _windowTracker.WindowSizeChanged += (_, size) => Dispatcher.Invoke(() =>
        {
            _logger.LogDebug("Overlay: WindowSizeChanged: {size}", size.ToString());
            Width = size.Width;
            Height = size.Height;
        });

        _globalHotkey.TabKeyPressed += (_, _) => Dispatcher.Invoke(() =>
        {
            Console.WriteLine("Overlay: HotKeyPressed");

            if (Visibility == Visibility.Visible)
            {
                HideOverlay();
                return;
            }

            if (!_windowTracker.IsWindowFocused()) return;

            ShowOverlay();
        });
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        _logger.LogDebug("Overlay: KeyDown: {key}", e.Key);
        base.OnKeyDown(e);
        if (e.Key == Key.F)
        {
            BlazorWebView.WebView.Focus();
        }
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

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        SetExtendedWindowStyle();
            
        BlazorWebView.WebView.DefaultBackgroundColor = Color.Transparent;
        BlazorWebView.WebView.NavigationCompleted += WebView_Loaded;
        BlazorWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
        Visibility = Visibility.Collapsed;
    }

    private void BlazorWebView_Initializing(object? sender, BlazorWebViewInitializingEventArgs e)
    {
        e.UserDataFolder = Path.Join(Constants.LocalAppDataPath, "WebView");
    }

    private void WebView_Loaded(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        // If we are running in a development/debugger mode, open dev tools to help out
        // if (Debugger.IsAttached) blazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
        BlazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
        BlazorWebView.Focus();
    }

    private void CoreWebView_Loaded(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        // BlazorWebView.WebView.CoreWebView2.SetVirtualHostNameToFolderMapping("resources.internal", "Resources", CoreWebView2HostResourceAccessKind.Allow);
        BlazorWebView.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        BlazorWebView.WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
    }

    /// <summary>
    /// Interface to hide the overlay from JS.
    /// Collapses the window. This will hide the window and give focus back to Star Citizen.
    /// </summary>
    public void Collapse()
    {
        Visibility = Visibility.Collapsed;
        // we switch focus back to Star Citizen because
        // otherwise the previously active window will
        // receive focus instead for some reason
        if (_currentWindowHWnd == HWND.Null) return;
        SetForegroundWindow(_currentWindowHWnd);
    }

    private void OnExitCommand(object sender, RoutedEventArgs e)
    {
        // Application.Current.Shutdown();
        _hostApplicationLifetime.StopApplication();
    }
}