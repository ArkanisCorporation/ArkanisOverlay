namespace Arkanis.Overlay.Components.Helpers;

using Arkanis.Overlay.Domain.Models.IconSelection;
using MudBlazor;
using MudBlazor.FontIcons.MaterialSymbols;

/// <summary>
/// Maps internal icon selections to actual MudBlazor icon strings.
/// </summary>
public static class MudBlazorIconMapping
{
    /// <summary>
    /// Converts an IconSelectionObject to a MudBlazor-compatible icon string.
    /// </summary>
    public static string GetIconString(IconSelectionObject iconSelection)
        => iconSelection.Type switch
        {
            IconType.MaterialSymbols => GetMaterialSymbolString(iconSelection.IconName),
            IconType.MaterialIcons => GetMaterialIconString(iconSelection.IconName),
            IconType.Custom => $"custom:{iconSelection.IconName}",
            _ => GetDefaultIconString(),
        };

    private static string GetMaterialIconString(string iconName)
        => iconName switch
        {
            // Commonly used icons in the current codebase
            "Search" => MudBlazor.Icons.Material.Filled.Search,
            "Add" => MudBlazor.Icons.Material.Filled.Add,
            "Remove" => MudBlazor.Icons.Material.Filled.Remove,
            "Settings" => MudBlazor.Icons.Material.Filled.Settings,
            "LocationOn" => MudBlazor.Icons.Material.Filled.LocationOn,
            "LocationOff" => MudBlazor.Icons.Material.Filled.LocationOff,
            "Warehouse" => MudBlazor.Icons.Material.Filled.Warehouse,
            "Storefront" => MudBlazor.Icons.Material.Filled.Storefront,
            "Store" => MudBlazor.Icons.Material.Filled.Store,
            "Groups" => MudBlazor.Icons.Material.Filled.Groups,
            "Diamond" => MudBlazor.Icons.Material.Filled.Diamond,
            "Rocket" => MudBlazor.Icons.Material.Filled.Rocket,
            "LocalShipping" => MudBlazor.Icons.Material.Filled.LocalShipping,
            "Category" => MudBlazor.Icons.Material.Filled.Category,
            "Topic" => MudBlazor.Icons.Material.Filled.Topic,
            "Domain" => MudBlazor.Icons.Material.Filled.Domain,
            "Public" => MudBlazor.Icons.Material.Filled.Public,
            "AddShoppingCart" => MudBlazor.Icons.Material.Filled.AddShoppingCart,
            "RemoveShoppingCart" => MudBlazor.Icons.Material.Filled.RemoveShoppingCart,
            "CarRental" => MudBlazor.Icons.Material.Filled.CarRental,
            _ => MudBlazor.Icons.Material.Filled.Square,
        };

    private static string GetMaterialSymbolString(string iconName)
        => iconName switch
        {
            // Commonly used icons in the current codebase
            "Search" => Outlined.Search,
            "FrameReload" => Outlined.FrameReload,
            "ChevronLeft" => Outlined.ChevronLeft,
            "OpenInBrowser" => Outlined.OpenInBrowser,
            "Notifications" => Outlined.Notifications,
            "Settings" => Outlined.Settings,
            "Deblur" => Outlined.Deblur,
            "Store" => Outlined.Store,
            "GarageDoor" => Outlined.GarageDoor,
            _ => Outlined.Square,
        };

    private static string GetDefaultIconString()
        => MudBlazor.Icons.Material.Filled.Square;
}
