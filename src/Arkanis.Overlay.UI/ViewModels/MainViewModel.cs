namespace Arkanis.Overlay.UI.ViewModels;

using System.Reactive;
using Pages;
using ReactiveUI;

public class MainViewModel : ViewModelBase, IScreen
{
    public RoutingState Router { get; } = new();

    public ReactiveCommand<Unit, IRoutableViewModel> GoToSearchPage { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoToInventoryPage { get; }

    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;

    public MainViewModel()
    {
        GoToSearchPage = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new SearchPageViewModel(this))
        );
        GoToInventoryPage = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new InventoryPageViewModel(this))
        );
    }
}
