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

    /// <summary>
    /// Converts an icon name string to a MudBlazor-compatible icon string.
    /// Tries Material Icons first, then Material Symbols.
    /// </summary>
    public static string GetIconString(string iconName)
    {
        if (string.IsNullOrWhiteSpace(iconName))
            return GetDefaultIconString();

        // Try Material Icons first (Filled variant)
        var materialIcon = GetMaterialIconString(iconName);
        if (materialIcon != GetDefaultIconString())
            return materialIcon;

        // Fallback to Material Symbols
        var materialSymbol = GetMaterialSymbolString(iconName);
        if (materialSymbol != GetDefaultIconString())
            return materialSymbol;

        return GetDefaultIconString();
    }

    /// <summary>
    /// Converts an icon name string to a MudBlazor-compatible icon string with custom icon support.
    /// Checks custom icons first, then falls back to standard mapping.
    /// </summary>
    public static string GetIconString(string iconName, IReadOnlyDictionary<string, string>? customIcons = null)
    {
        if (string.IsNullOrWhiteSpace(iconName))
            return GetDefaultIconString();

        // Check for custom icon preference first
        if (customIcons?.TryGetValue(iconName, out var customIcon) == true)
        {
            return GetIconString(customIcon);
        }

        // Fall back to standard mapping
        return GetIconString(iconName);
    }

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
            
            // Additional icons found in codebase
            "ScreenshotMonitor" => MudBlazor.Icons.Material.Filled.ScreenshotMonitor,
            "InstallDesktop" => MudBlazor.Icons.Material.Filled.InstallDesktop,
            "Web" => MudBlazor.Icons.Material.Filled.Web,
            "Flight" => MudBlazor.Icons.Material.Filled.Flight,
            "Calculate" => MudBlazor.Icons.Material.Filled.Calculate,
            "ContentCopy" => MudBlazor.Icons.Material.Filled.ContentCopy,
            "Info" => MudBlazor.Icons.Material.Filled.Info,
            "Square" => MudBlazor.Icons.Material.Filled.Square,
            
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
