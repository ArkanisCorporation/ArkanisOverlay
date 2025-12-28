namespace Arkanis.Overlay.UI.Views.Pages;

using ViewModels;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ViewModels.Pages;

public partial class SearchPageView : ReactiveUserControl<SearchPageViewModel>
{
    public SearchPageView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}

