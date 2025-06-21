namespace Arkanis.Overlay.Components.Services;

using Domain.Abstractions.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
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
            GetUpdateCountAsync = _ => ValueTask.FromResult(4),
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

        public string GetAbsoluteUri(NavigationManager  navigationManager)
            => navigationManager.ToAbsoluteUri(Url).ToString();

        public Func<IServiceProvider, ValueTask<int>> GetUpdateCountAsync { get; set; } = _ => ValueTask.FromResult(0);
    }
}
