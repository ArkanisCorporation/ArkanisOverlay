namespace Arkanis.Overlay.UI.Windows;

using ViewModels;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    public MainWindow()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}
