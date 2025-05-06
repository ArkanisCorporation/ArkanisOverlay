namespace Arkanis.Overlay.Application.UI.Windows;

using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Services;

public partial class PreferencesWindow : Window
{
    public PreferencesWindow(WindowsPreferencesControls PreferencesControls)
    {
        InitializeComponent();

        PreferencesControls.PreferencesClosed += (sender, args) =>
        {
            Dispatcher.Invoke(Close);
        };
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
