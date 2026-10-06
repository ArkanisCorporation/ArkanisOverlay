namespace Arkanis.Overlay.Host.Desktop.UI.Windows;

using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Services;

public partial class AboutWindow
{
    private readonly WindowControls<AboutWindow> _windowControls;

    public AboutWindow(WindowControls<AboutWindow> windowControls)
    {
        InitializeComponent();
        _windowControls = windowControls;
        _windowControls.Bind(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        _windowControls.Unbind(this);
        base.OnClosed(e);
    }

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        BlazorWebView.WebView.DefaultBackgroundColor = Color.Transparent;
        BlazorWebView.WebView.NavigationCompleted += WebView_Loaded;
        BlazorWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
    }

    private void CoreWebView_Loaded(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        // BlazorWebView.WebView.CoreWebView2.SetVirtualHostNameToFolderMapping("resources.internal", "Resources", CoreWebView2HostResourceAccessKind.Allow);
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
