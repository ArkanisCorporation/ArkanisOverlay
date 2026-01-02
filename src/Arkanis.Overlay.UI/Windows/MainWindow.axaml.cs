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
            // currently just a test, will be used later for HUD mode / window
            var windowUtils = Services.GetRequiredService<IWindowUtils>();
            windowUtils.EnableClickThrough(this);
            windowUtils.DisableClickThrough(this);
        });
        AvaloniaXamlLoader.Load(this);
    }
}
