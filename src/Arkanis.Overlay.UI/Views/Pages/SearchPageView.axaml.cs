namespace Arkanis.Overlay.UI.Views.Pages;

using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using ViewModels.Pages;

public partial class SearchPageView : ReactiveUserControl<SearchPageViewModel>
{
    public SearchPageView()
        => AvaloniaXamlLoader.Load(this);
}

