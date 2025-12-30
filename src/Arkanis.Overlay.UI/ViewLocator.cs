namespace Arkanis.Overlay.UI;

using System;
using ReactiveUI;
using ViewModels.Pages;
using Views.Pages;

public class ViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null)
        => viewModel switch
        {
            SearchPageViewModel context => new SearchPageView { DataContext = context },
            InventoryPageViewModel context => new InventoryPageView { DataContext = context },
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel)),
        };
}
