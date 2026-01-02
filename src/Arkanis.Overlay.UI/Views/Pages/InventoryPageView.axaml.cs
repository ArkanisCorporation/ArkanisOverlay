namespace Arkanis.Overlay.UI.Views.Pages;

using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ViewModels.Pages;

public partial class InventoryPageView : ReactiveUserControl<InventoryPageViewModel>
{
    public InventoryPageView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}
