namespace Arkanis.Overlay.UI.ViewModels;

using System;
using System.Linq;
using Avalonia.Input;
using DynamicData.Binding;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Pages;
using ReactiveUI;
using Utils;
using static MoreLinq.Extensions.ForEachExtension;

public class MainViewModel : ViewModelBase, IScreen
{
    public MainViewModel()
    {
        MenuItems = new ObservableCollectionExtended<AActionEntry>
        {
            new CommandActionEntry
            {
                Name = "Search",
                Description = "Find anything you're looking for.",
                Icon = new MaterialLibrarySymbol(MaterialIconKind.Search),
                Command = ReactiveCommand.CreateFromObservable(CreateAndNavigateWith<SearchPageViewModel>),
            },
            new CommandActionEntry
            {
                Name = "Hub",
                Description = "See what's going on around you.",
                Icon = new MaterialLibrarySymbol(MaterialIconKind.Hub),
                Command = ReactiveCommand.CreateFromObservable(CreateAndNavigateWith<HubPageViewModel>),
                IsInDevelopment = true,
            },
            new CommandActionEntry
            {
                Name = "Close",
                Description = "Close the Overlay.",
                Icon = new MaterialLibrarySymbol(MaterialIconKind.Close),
                // TODO: Implement Close Command
            },
            new CommandActionEntry
            {
                Name = "Inventory",
                Description = "Track and manage your Inventory.",
                Icon = new MaterialLibrarySymbol(MaterialIconKind.Warehouse),
                Command = ReactiveCommand.CreateFromObservable(CreateAndNavigateWith<InventoryPageViewModel>),
                IsInDevelopment = true,
            },
            new CommandActionEntry
            {
                Name = "Trade",
                Description = "Plan your next Haul.",
                Icon = new MaterialLibrarySymbol(MaterialIconKind.Trolley),
                Command = ReactiveCommand.CreateFromObservable(CreateAndNavigateWith<TradePageViewModel>),
                IsInDevelopment = true,
            },
            new CommandActionEntry
            {
                Name = "Mining",
                Description = "Manage your Mining Operations.",
                Icon = new MaterialLibrarySymbol(MaterialIconKind.Pickaxe),
                Command = ReactiveCommand.CreateFromObservable(CreateAndNavigateWith<MiningPageViewModel>),
                IsInDevelopment = true,
            },
            new CommandActionEntry
            {
                Name = "Market",
                Description = "Trade with other players.",
                Icon = new MaterialLibrarySymbol(MaterialIconKind.Store),
                Command = ReactiveCommand.CreateFromObservable(CreateAndNavigateWith<MarketPageViewModel>),
                IsInDevelopment = true,
            },
            new CommandActionEntry
            {
                Name = "Hangar",
                Description = "Manage your Fleet.",
                Icon = new MaterialLibrarySymbol(MaterialIconKind.Garage),
                Command = ReactiveCommand.CreateFromObservable(CreateAndNavigateWith<HangarPageViewModel>),
                IsInDevelopment = true,
            },
            new CommandActionEntry
            {
                Name = "Org",
                Description = "Manage your Organization.",
                Icon = new MaterialLibrarySymbol(MaterialIconKind.PeopleGroup),
                Command = ReactiveCommand.CreateFromObservable(CreateAndNavigateWith<OrgPageViewModel>),
                IsInDevelopment = true,
            },
            new SeparatorActionEntry(),
            new CommandActionEntry
            {
                Name = "Settings",
                Description = "Configure the Overlay.",
                Icon = new MaterialLibrarySymbol(MaterialIconKind.Settings),
                Command = ReactiveCommand.CreateFromObservable(CreateAndNavigateWith<SettingsPageViewModel>),
                Shortcut = new KeyGesture(Key.F12),
                IsInDevelopment = true,
            },
        };

        // Assign F1-F11 shortcuts to the first 11 menu items without a shortcut
        MenuItems.OfType<ActionEntry>()
            .Take(11)
            .Where(entry => entry.Shortcut is null)
            .ForEach((entry, index) => entry.Shortcut = new KeyGesture(Key.F1 + index));
    }

    public required IServiceProvider Services { get; init; }

    private PageViewModelFactory PageViewModelFactory
        => Services.GetRequiredService<PageViewModelFactory>();

    public IObservableCollection<AActionEntry> MenuItems { get; }

    public RoutingState Router { get; } = new();

    private IObservable<IRoutableViewModel> CreateAndNavigateWith<TViewModel>() where TViewModel : PageViewModelBase
    {
        var viewModel = PageViewModelFactory.Create<TViewModel>(this);
        return Router.Navigate.Execute(viewModel);
    }
}
