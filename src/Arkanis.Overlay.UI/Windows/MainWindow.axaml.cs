namespace Arkanis.Overlay.UI.Windows;

using System;
using ViewModels;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Native;
using ReactiveUI;
using ReactiveUI.Avalonia;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    private IServiceProvider Services
        => ViewModel?.Services ?? throw new InvalidOperationException($"{nameof(ViewModel)} for {this} has not been set.");

    public MainWindow()
    {

        this.WhenActivated(disposables =>
        {
            var windowUtils = Services.GetRequiredService<IWindowUtils>();
            windowUtils.SetExtendedStyle(this, WindowStyle.Transparent);
            // windowUtils.SetExtendedStyle(this, WindowStyle.ToolWindow);
        });
        AvaloniaXamlLoader.Load(this);
    }
}
