namespace Arkanis.Overlay.Infrastructure.Services.IconManagement;

using Arkanis.Overlay.Domain.Abstractions.Services;
using Arkanis.Overlay.Domain.Constants;
using Arkanis.Overlay.Domain.Enums;
using Arkanis.Overlay.Domain.Models.IconSelection;

public sealed class IconService()
    : IIconService
{
    private const string DefaultIcon = "Icons.Material.Filled.Square";

    public IconSelectionObject GetIconSelection(string iconIdentifier)
    {
        // Create an AppIcon from the identifier and convert to IconSelectionObject
        var appIcon = CreateAppIconFromIdentifier(iconIdentifier);
        return appIcon.ToIconSelectionObject();
    }

    public IconSelectionObject GetIconSelectionFor<T>(T value)
        => value switch
        {
            GameEntityCategory x => GetIconSelectionFor(x),
            PriceType x => GetIconSelectionFor(x),
            _ => GetDefaultIconSelection(),
        };

    private static string GetIconNameForType(IconSelectionObject selection)
        => GetIconNameForType(selection, selection.IconName);

    private static string GetIconNameForType(IconSelectionObject selection, string iconName)
        => selection.Type switch
        {
            IconType.MaterialSymbols => iconName,
            IconType.MaterialIcons => iconName,
            IconType.Custom => $"custom:{iconName}",
            _ => OverlayIcons.Status.Square,
        };

    private static IconSelectionObject GetIconSelectionFor(PriceType value)
        => value switch
        {
            PriceType.Buy => CreateIconSelection(OverlayIcons.Trade.AddShoppingCart, IconCategory.Trade, "Buy or purchase"),
            PriceType.Sell => CreateIconSelection(OverlayIcons.Trade.RemoveShoppingCart, IconCategory.Trade, "Sell or remove"),
            PriceType.Rent => CreateIconSelection(OverlayIcons.Trade.CarRental, IconCategory.Trade, "Rent or lease"),
            _ => GetDefaultIconSelection(),
        };

    private static IconSelectionObject GetIconSelectionFor(GameEntityCategory value)
        => value switch
        {
            GameEntityCategory.Commodity => CreateIconSelection(OverlayIcons.GameEntity.Diamond, IconCategory.GameEntity, "Commodity or valuable item"),
            GameEntityCategory.SpaceShip => CreateIconSelection(OverlayIcons.GameEntity.Rocket, IconCategory.GameEntity, "Space ship"),
            GameEntityCategory.GroundVehicle => CreateIconSelection(OverlayIcons.GameEntity.LocalShipping, IconCategory.GameEntity, "Ground vehicle"),
            GameEntityCategory.Item => CreateIconSelection(OverlayIcons.GameEntity.Category, IconCategory.GameEntity, "Item category"),
            GameEntityCategory.ProductCategory => CreateIconSelection(OverlayIcons.GameEntity.Topic, IconCategory.GameEntity, "Product category"),
            GameEntityCategory.Company => CreateIconSelection(OverlayIcons.GameEntity.Domain, IconCategory.GameEntity, "Company or organization"),
            GameEntityCategory.Location => CreateIconSelection(OverlayIcons.GameEntity.Public, IconCategory.GameEntity, "Location or place"),
            _ => GetDefaultIconSelection(),
        };

    private static IconSelectionObject CreateIconSelection(string iconName, IconCategory category, string description)
        => new()
        {
            Category = category,
            Type = IconType.MaterialIcons,
            IconName = iconName,
            Description = description,
            IsActive = true,
            Priority = 0,
            FallbackIcons = []
        };

    private static IconSelectionObject GetDefaultIconSelection()
        => CreateIconSelection(OverlayIcons.Status.Square, IconCategory.Status, "Default icon");

    private static IconCategory DetermineCategoryFromIdentifier(string identifier)
    {
        // Determine category based on the identifier prefix
        if (identifier.StartsWith("Navigation.", StringComparison.OrdinalIgnoreCase)) return IconCategory.Navigation;
        if (identifier.StartsWith("Actions.", StringComparison.OrdinalIgnoreCase)) return IconCategory.Action;
        if (identifier.StartsWith("System.", StringComparison.OrdinalIgnoreCase)) return IconCategory.System;
        if (identifier.StartsWith("GameEntity.", StringComparison.OrdinalIgnoreCase)) return IconCategory.GameEntity;
        if (identifier.StartsWith("Trade.", StringComparison.OrdinalIgnoreCase)) return IconCategory.Trade;
        if (identifier.StartsWith("Status.", StringComparison.OrdinalIgnoreCase)) return IconCategory.Status;
        
        return IconCategory.Status; // Default fallback
    }

    private static string GetDescriptionForIdentifier(string identifier)
    {
        // Extract a human-readable description from the identifier
        var parts = identifier.Split('.');
        if (parts.Length >= 2)
        {
            var iconName = parts[^1]; // Last part
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
                _ => $"Icon: {iconName}"
            };
        }
        
        return $"Icon: {identifier}";
    }

    private static AppIcon CreateAppIconFromIdentifier(string identifier)
    {
        var category = DetermineCategoryFromIdentifier(identifier);
        var description = GetDescriptionForIdentifier(identifier);
        
        // Determine the icon type and create appropriate AppIcon
        if (identifier.Contains("MaterialSymbols") || identifier.Contains("Symbol"))
        {
            return new MaterialSymbolIcon
            {
                Identifier = identifier,
                Category = category,
                Description = description,
                IconName = ExtractIconName(identifier),
                Variant = "Outlined",
                IsActive = true,
                Priority = 0,
                FallbackIcons = []
            };
        }
        
        if (identifier.Contains("Custom"))
        {
            return new CustomIcon
            {
                Identifier = identifier,
                Category = category,
                Description = description,
                Implementation = identifier,
                Format = "Custom",
                IsActive = true,
                Priority = 0,
                FallbackIcons = []
            };
        }
        
        // Default to MaterialIcon
        return new MaterialIcon
        {
            Identifier = identifier,
            Category = category,
            Description = description,
            IconName = ExtractIconName(identifier),
            Variant = "Filled",
            IsActive = true,
            Priority = 0,
            FallbackIcons = []
        };
    }

    private static string ExtractIconName(string identifier)
    {
        // Extract the actual icon name from the identifier
        var parts = identifier.Split('.');
        return parts.Length >= 2 ? parts[^1] : identifier;
    }
}


