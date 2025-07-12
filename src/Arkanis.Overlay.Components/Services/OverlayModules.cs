namespace Arkanis.Overlay.Components.Services;

using Domain.Abstractions.Services;
using Domain.Models.Keyboard;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using MudBlazor;
using MudBlazor.FontIcons.MaterialSymbols;

public class OverlayModules
{
    private readonly ICollection<Entry> _modules =
    [
        new()
        {
            Url = "/search",
            Name = "Search",
            Icon = Outlined.Search,
        },
        new()
        {
            Url = "/inventory",
            Name = "Inventory",
            Icon = Icons.Material.Filled.Warehouse,
            GetChangeToken = serviceProvider => serviceProvider.GetRequiredService<IInventoryManager>().ChangeToken,
            GetUpdateCountAsync = async serviceProvider =>
            {
                var inventoryManager = serviceProvider.GetRequiredService<IInventoryManager>();
                return await inventoryManager.GetUnassignedCountAsync();
            },
        },
        new()
        {
            Url = "/trade",
            Name = "Trade",
            Icon = Outlined.Storefront,
            GetChangeToken = serviceProvider => serviceProvider.GetRequiredService<ITradeRunManager>().ChangeToken,
            GetUpdateCountAsync = async serviceProvider =>
            {
                var inventoryManager = serviceProvider.GetRequiredService<ITradeRunManager>();
                return await inventoryManager.GetInProgressCountAsync();
            },
        },
        new()
        {
            Url = "/mining",
            Name = "Mining",
            Icon = Outlined.Deblur,
            Disabled = true,
        },
        new()
        {
            Url = "/market",
            Name = "Market",
            Icon = Outlined.Store,
            Disabled = true,
        },
        new()
        {
            Url = "/hangar",
            Name = "Hangar",
            Icon = Outlined.GarageDoor,
            Disabled = true,
        },
        new()
        {
            Url = "/org",
            Name = "Org",
            Icon = Icons.Material.Filled.Groups,
            Disabled = true,
        },
        new()
        {
            Url = "/settings",
            Name = "Settings",
            Icon = Outlined.Settings,
            ShortcutOverride = KeyboardKey.F12,
            Disabled = true,
        },
    ];

    public ICollection<Entry> GetAll()
        => _modules;

    public class Entry
    {
        public required string Url { get; init; }
        public required string Name { get; init; }

        public bool Disabled { get; init; }
        public string Icon { get; init; } = Icons.Material.Filled.ViewModule;
        public KeyboardKey? ShortcutOverride { get; init; }

        public Func<IServiceProvider, IChangeToken> GetChangeToken { get; set; } =
            _ => NullChangeToken.Singleton;

        public Func<IServiceProvider, ValueTask<int>> GetUpdateCountAsync { get; set; } =
            _ => ValueTask.FromResult(0);

        public string GetAbsoluteUri(NavigationManager navigationManager)
            => navigationManager.ToAbsoluteUri(Url).ToString();
    }
}
