namespace Arkanis.Overlay.Infrastructure.Services.IconManagement;

using Arkanis.Overlay.Domain.Abstractions.Services;
using Arkanis.Overlay.Domain.Constants;
using Arkanis.Overlay.Domain.Models.IconSelection;
using System.Reflection;

/// <summary>
/// Service for discovering and generating icon lists from constants using reflection.
/// </summary>
public sealed class IconDiscoveryService : IIconDiscoveryService
{
    private readonly Lazy<IReadOnlyList<IconSelectionObject>> _allIcons;

    public IconDiscoveryService()
    {
        _allIcons = new Lazy<IReadOnlyList<IconSelectionObject>>(DiscoverAllIcons);
    }

    /// <summary>
    /// Gets all available icons discovered from constants.
    /// </summary>
    public IReadOnlyList<IconSelectionObject> GetAllIcons() => _allIcons.Value;

    /// <summary>
    /// Gets icons filtered by category.
    /// </summary>
    public IReadOnlyList<IconSelectionObject> GetIconsByCategory(IconCategory category)
        => GetAllIcons().Where(i => i.Category == category).ToList();

    /// <summary>
    /// Gets icons filtered by type.
    /// </summary>
    public IReadOnlyList<IconSelectionObject> GetIconsByType(IconType type)
        => GetAllIcons().Where(i => i.Type == type).ToList();

    /// <summary>
    /// Gets icons filtered by category and type.
    /// </summary>
    public IReadOnlyList<IconSelectionObject> GetIconsByCategoryAndType(IconCategory category, IconType type)
        => GetAllIcons().Where(i => i.Category == category && i.Type == type).ToList();

    /// <summary>
    /// Searches icons by name or description.
    /// </summary>
    public IReadOnlyList<IconSelectionObject> SearchIcons(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return GetAllIcons();
        }

        var searchLower = searchTerm.ToLowerInvariant();
        return GetAllIcons()
            .Where(i => i.IconName.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                       i.Description.Contains(searchLower, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static List<IconSelectionObject> DiscoverAllIcons()
    {
        var icons = new List<IconSelectionObject>();
        var overlayIconsType = typeof(OverlayIcons);

        // Get all nested classes (Navigation, Actions, System, etc.)
        var nestedTypes = overlayIconsType.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var nestedType in nestedTypes)
        {
            var category = DetermineCategoryFromType(nestedType);
            
            // Get all constant fields from the nested type
            var constants = nestedType.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string));

            foreach (var constant in constants)
            {
                var iconIdentifier = (string)constant.GetValue(null)!;
                var iconName = ExtractIconName(iconIdentifier);
                
                icons.Add(new IconSelectionObject
                {
                    Category = category,
                    Type = DetermineIconType(iconName),
                    IconName = iconName,
                    Description = GetDescriptionForIconName(iconName),
                    IsActive = true,
                    Priority = 0,
                    FallbackIcons = []
                });
            }
        }

        return icons.OrderBy(i => i.Category).ThenBy(i => i.IconName).ToList();
    }

    private static IconCategory DetermineCategoryFromType(Type type)
    {
        return type.Name switch
        {
            nameof(OverlayIcons.Navigation) => IconCategory.Navigation,
            nameof(OverlayIcons.Actions) => IconCategory.Action,
            nameof(OverlayIcons.System) => IconCategory.System,
            nameof(OverlayIcons.GameEntity) => IconCategory.GameEntity,
            nameof(OverlayIcons.Trade) => IconCategory.Trade,
            nameof(OverlayIcons.Status) => IconCategory.Status,
            _ => IconCategory.Status
        };
    }

    private static IconType DetermineIconType(string iconName)
    {
        // For now, default to MaterialIcons
        // This could be enhanced to determine type based on icon name patterns
        return IconType.MaterialIcons;
    }

    private static string ExtractIconName(string iconIdentifier)
    {
        // Extract the actual icon name from the identifier
        var parts = iconIdentifier.Split('.');
        return parts.Length >= 2 ? parts[^1] : iconIdentifier;
    }

    private static string GetDescriptionForIconName(string iconName)
    {
        return iconName switch
        {
            "Search" => "Search functionality",
            "Add" => "Add new item",
            "Remove" => "Remove item",
            "Settings" => "Application settings",
            "Calculate" => "Calculate or compute",
            "ContentCopy" => "Copy to clipboard",
            "ScreenshotMonitor" => "Overlay or monitor",
            "InstallDesktop" => "Install application",
            "Web" => "Web or external link",
            "Flight" => "Flight or travel",
            "Info" => "Information",
            "Diamond" => "Commodity or valuable item",
            "Rocket" => "Space ship",
            "LocalShipping" => "Ground vehicle",
            "Category" => "Item category",
            "Topic" => "Product category",
            "Domain" => "Company or organization",
            "Public" => "Location or place",
            "AddShoppingCart" => "Buy or purchase",
            "RemoveShoppingCart" => "Sell or remove",
            "CarRental" => "Rent or lease",
            "Square" => "Default icon",
            "ChevronLeft" => "Navigate left",
            "OpenInBrowser" => "Open in browser",
            "FrameReload" => "Reload or refresh",
            "Notifications" => "Notifications",
            "Deblur" => "Deblur or focus",
            "Store" => "Store or shop",
            "GarageDoor" => "Garage or storage",
            "LocationOn" => "Location enabled",
            "LocationOff" => "Location disabled",
            "Storefront" => "Storefront or business",
            "Warehouse" => "Warehouse or storage",
            "Groups" => "Groups or teams",
            _ => $"Icon: {iconName}"
        };
    }
}
