namespace Arkanis.Overlay.Components.Services;

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
    ];

    public ICollection<Entry> GetAll()
        => _modules;

    public class Entry
    {
        public required string Url { get; init; }
        public required string Name { get; init; }

        public string Icon { get; init; } = Icons.Material.Filled.ViewModule;
    }
}
