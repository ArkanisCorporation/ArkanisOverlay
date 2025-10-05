namespace Arkanis.Overlay.Components.Helpers;

using Arkanis.Overlay.Domain.Constants;
using Arkanis.Overlay.Domain.Models.IconSelection;
using MudBlazor.FontIcons.MaterialSymbols;

/// <summary>
/// Maps internal icon selections to actual MudBlazor icon strings.
/// </summary>
public static class MudBlazorIconMapping
{

    /// <summary>
    /// Converts an IconSelectionObject to a MudBlazor-compatible icon string.
    /// Only maps to MudBlazor icons if the category and type are appropriate.
    /// </summary>
    public static string GetIconString(IconSelectionObject iconSelection)
    {
        // Only handle MudBlazor-compatible icon types
        if (iconSelection.Type is not IconType.MaterialIcons and not IconType.MaterialSymbols)
        {
            return DefaultIconString;
        }

        return iconSelection.Type switch
        {
            IconType.MaterialSymbols => GetMaterialSymbolString(iconSelection.IconName),
            IconType.MaterialIcons => GetMaterialIconString(iconSelection.IconName),
            IconType.Custom => throw new NotImplementedException(),
            IconType.Undefined => throw new NotImplementedException(),
            _ => DefaultIconString,
        };
    }

    /// <summary>
    /// Converts an icon name string to a MudBlazor-compatible icon string.
    /// Tries Material Icons first, then Material Symbols.
    /// </summary>
    public static string GetIconString(string iconName)
    {
        var defaultIcon = DefaultIconString;
        if (string.IsNullOrWhiteSpace(iconName))
        {
            return defaultIcon;
        }

        // Try Material Icons first (Filled variant)
        var materialIcon = GetMaterialIconString(iconName);
        if (materialIcon != defaultIcon)
        {
            return materialIcon;
        }

        // Fallback to Material Symbols
        var materialSymbol = GetMaterialSymbolString(iconName);
        if (materialSymbol != defaultIcon)
        {
            return materialSymbol;
        }

        return defaultIcon;
    }

    /// <summary>
    /// Converts an icon name string to a MudBlazor-compatible icon string with custom icon support.
    /// Checks custom icons first, then falls back to standard mapping.
    /// </summary>
    public static string GetIconString(string iconName, IReadOnlyDictionary<string, string>? customIcons = null)
    {
        if (string.IsNullOrWhiteSpace(iconName))
        {
            return DefaultIconString;
        }

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
            // Navigation icons
            OverlayIcons.Navigation.Search => MudBlazor.Icons.Material.Filled.Search,
            OverlayIcons.Navigation.Web => MudBlazor.Icons.Material.Filled.Web,

            // Action icons
            OverlayIcons.Actions.Add => MudBlazor.Icons.Material.Filled.Add,
            OverlayIcons.Actions.Remove => MudBlazor.Icons.Material.Filled.Remove,
            OverlayIcons.Actions.Calculate => MudBlazor.Icons.Material.Filled.Calculate,
            OverlayIcons.Actions.ContentCopy => MudBlazor.Icons.Material.Filled.ContentCopy,

            // System icons
            OverlayIcons.System.Settings => MudBlazor.Icons.Material.Filled.Settings,
            OverlayIcons.System.ScreenshotMonitor => MudBlazor.Icons.Material.Filled.ScreenshotMonitor,
            OverlayIcons.System.InstallDesktop => MudBlazor.Icons.Material.Filled.InstallDesktop,

            // Game entity icons
            OverlayIcons.GameEntity.Commodity => MudBlazor.Icons.Material.Filled.Diamond,
            OverlayIcons.GameEntity.SpaceShip => MudBlazor.Icons.Material.Filled.Rocket,
            OverlayIcons.GameEntity.GroundVehicle => MudBlazor.Icons.Material.Filled.LocalShipping,
            OverlayIcons.GameEntity.Item => MudBlazor.Icons.Material.Filled.Category,
            OverlayIcons.GameEntity.ProductCategory => MudBlazor.Icons.Material.Filled.Topic,
            OverlayIcons.GameEntity.Company => MudBlazor.Icons.Material.Filled.Domain,
            OverlayIcons.GameEntity.Location => MudBlazor.Icons.Material.Filled.Public,
            OverlayIcons.GameEntity.Flight => MudBlazor.Icons.Material.Filled.Flight,
            OverlayIcons.GameEntity.Store => MudBlazor.Icons.Material.Filled.Store,

            // Trade icons
            OverlayIcons.Trade.AddShoppingCart => MudBlazor.Icons.Material.Filled.AddShoppingCart,
            OverlayIcons.Trade.RemoveShoppingCart => MudBlazor.Icons.Material.Filled.RemoveShoppingCart,
            OverlayIcons.Trade.CarRental => MudBlazor.Icons.Material.Filled.CarRental,
            OverlayIcons.Trade.Storefront => MudBlazor.Icons.Material.Filled.Storefront,
            OverlayIcons.Trade.Warehouse => MudBlazor.Icons.Material.Filled.Warehouse,

            // Location icons
            OverlayIcons.Location.LocationOn => MudBlazor.Icons.Material.Filled.LocationOn,
            OverlayIcons.Location.LocationOff => MudBlazor.Icons.Material.Filled.LocationOff,

            // Status icons
            OverlayIcons.Status.Info => MudBlazor.Icons.Material.Filled.Info,
            OverlayIcons.Status.Square => MudBlazor.Icons.Material.Filled.Square,

            // Social icons
            OverlayIcons.Social.Groups => MudBlazor.Icons.Material.Filled.Groups,

            _ => MudBlazor.Icons.Material.Filled.Square,
        };

    private static string GetMaterialSymbolString(string iconName)
        => iconName switch
        {
            // Navigation icons
            OverlayIcons.Navigation.Search => Outlined.Search,
            OverlayIcons.Navigation.ChevronLeft => Outlined.ChevronLeft,
            OverlayIcons.Navigation.OpenInBrowser => Outlined.OpenInBrowser,

            // Action icons
            OverlayIcons.Actions.FrameReload => Outlined.FrameReload,

            // System icons
            OverlayIcons.System.Notifications => Outlined.Notifications,
            OverlayIcons.System.Settings => Outlined.Settings,
            OverlayIcons.System.Deblur => Outlined.Deblur,

            // Game entity icons
            OverlayIcons.GameEntity.Store => Outlined.Store,
            OverlayIcons.GameEntity.GarageDoor => Outlined.GarageDoor,

            _ => Outlined.Square,
        };

    public static string DefaultIconString => MudBlazor.Icons.Material.Filled.Square;
}
