namespace Arkanis.Overlay.UI;

using System;
using Models.Pages;
using ReactiveUI;
using ViewModels;
using ViewModels.Pages;
using Views;
using Views.Pages;

public class AppViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null)
        => viewModel switch
        {
            SearchPageViewModel context => new SearchPageView { DataContext = context },
            InventoryPageViewModel context => new InventoryPageView { DataContext = context },
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel)),
        };
}
