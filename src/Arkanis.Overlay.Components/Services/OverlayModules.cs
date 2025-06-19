namespace Arkanis.Overlay.Components.Services;

using Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

public class OverlayModules
{
    private readonly ICollection<Entry> _modules =
    [
        new()
        {
            Url = "/search",
            Name = "Search",
            Icon = Icons.Material.Filled.Search,
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
            Icon = Icons.Material.Filled.Storefront,
        },
        new()
        {
            Url = "/mining",
            Name = "Mining",
            Icon = Icons.Material.Filled.Deblur,
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

        public Func<IServiceProvider, ValueTask<int>> GetUpdateCountAsync { get; set; } = _ => ValueTask.FromResult(0);
    }
}
