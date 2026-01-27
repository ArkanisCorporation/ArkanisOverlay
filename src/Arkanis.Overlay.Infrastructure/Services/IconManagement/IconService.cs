namespace Arkanis.Overlay.Infrastructure.Services.IconManagement;

using System.Reflection;
using Arkanis.Overlay.Domain.Abstractions.Services;
using Arkanis.Overlay.Domain.Constants;
using Arkanis.Overlay.Domain.Enums;
using Arkanis.Overlay.Domain.Models.IconSelection;

public sealed class IconService()
    : IIconService
{
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

    private static IconSelectionObject GetIconSelectionFor(PriceType value)
        => value switch
        {
            PriceType.Buy => CreateIconSelectionWithReflection(OverlayIcons.Trade.AddShoppingCart, IconCategory.Trade),
            PriceType.Sell => CreateIconSelectionWithReflection(OverlayIcons.Trade.RemoveShoppingCart, IconCategory.Trade),
            PriceType.Rent => CreateIconSelectionWithReflection(OverlayIcons.Trade.CarRental, IconCategory.Trade),
            PriceType.Undefined => throw new NotImplementedException(),
            _ => GetDefaultIconSelection(),
        };

    private static IconSelectionObject GetIconSelectionFor(GameEntityCategory value)
        => value switch
        {
            GameEntityCategory.Commodity => CreateIconSelectionWithReflection(OverlayIcons.GameEntity.Commodity, IconCategory.GameEntity),
            GameEntityCategory.SpaceShip => CreateIconSelectionWithReflection(OverlayIcons.GameEntity.SpaceShip, IconCategory.GameEntity),
            GameEntityCategory.GroundVehicle => CreateIconSelectionWithReflection(OverlayIcons.GameEntity.GroundVehicle, IconCategory.GameEntity),
            GameEntityCategory.Item => CreateIconSelectionWithReflection(OverlayIcons.GameEntity.Item, IconCategory.GameEntity),
            GameEntityCategory.ProductCategory => CreateIconSelectionWithReflection(OverlayIcons.GameEntity.ProductCategory, IconCategory.GameEntity),
            GameEntityCategory.Company => CreateIconSelectionWithReflection(OverlayIcons.GameEntity.Company, IconCategory.GameEntity),
            GameEntityCategory.Location => CreateIconSelectionWithReflection(OverlayIcons.GameEntity.Location, IconCategory.GameEntity),
            GameEntityCategory.Undefined => throw new NotImplementedException(),
            GameEntityCategory.ItemTrait => throw new NotImplementedException(),
            GameEntityCategory.TradeRoute => throw new NotImplementedException(),
            GameEntityCategory.Price => throw new NotImplementedException(),
            _ => GetDefaultIconSelection(),
        };

    private static IconSelectionObject CreateIconSelectionWithReflection(string iconName, IconCategory? overrideCategory = null)
    {
        // Use reflection to find the field with IconDescriptionAttribute
        var field = FindFieldWithIconName(iconName);
        var attribute = field?.GetCustomAttribute<IconDescriptionAttribute>();

        // Determine category based on the icon identifier prefix, or use override if provided
        var category = overrideCategory ?? DetermineCategoryFromIconName(iconName);

        return CreateIconSelection(
            iconName,
            category,
            attribute?.Description ?? $"Icon: {iconName}"
        );
    }

    private static IconCategory DetermineCategoryFromIconName(string iconName)
        => iconName switch
        {
            _ when iconName.StartsWith("Navigation.", StringComparison.OrdinalIgnoreCase) => IconCategory.Navigation,
            _ when iconName.StartsWith("Actions.", StringComparison.OrdinalIgnoreCase) => IconCategory.Action,
            _ when iconName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) => IconCategory.System,
            _ when iconName.StartsWith("GameEntity.", StringComparison.OrdinalIgnoreCase) => IconCategory.GameEntity,
            _ when iconName.StartsWith("Trade.", StringComparison.OrdinalIgnoreCase) => IconCategory.Trade,
            _ when iconName.StartsWith("Status.", StringComparison.OrdinalIgnoreCase) => IconCategory.Status,
            _ => IconCategory.Status // Default fallback
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

    private static IconCategory DetermineCategoryFromIdentifier(string identifier) =>
        identifier switch
        {
            _ when identifier.StartsWith("Navigation.", StringComparison.OrdinalIgnoreCase) => IconCategory.Navigation,
            _ when identifier.StartsWith("Actions.", StringComparison.OrdinalIgnoreCase) => IconCategory.Action,
            _ when identifier.StartsWith("System.", StringComparison.OrdinalIgnoreCase) => IconCategory.System,
            _ when identifier.StartsWith("GameEntity.", StringComparison.OrdinalIgnoreCase) => IconCategory.GameEntity,
            _ when identifier.StartsWith("Trade.", StringComparison.OrdinalIgnoreCase) => IconCategory.Trade,
            _ when identifier.StartsWith("Status.", StringComparison.OrdinalIgnoreCase) => IconCategory.Status,
            _ => IconCategory.Status // Default fallback
        };

    private static string GetDescriptionForIdentifier(string identifier)
    {
        // Extract a human-readable description from the identifier
        var parts = identifier.Split('.');
        if (parts.Length >= 2)
        {
            var iconName = parts[^1]; // Last part

            // Use reflection to find the field with IconDescriptionAttribute
            var field = FindFieldWithIconName(iconName);
            if (field != null)
            {
                var attribute = field.GetCustomAttribute<IconDescriptionAttribute>();
                if (attribute != null)
                {
                    return attribute.Description;
                }
            }

            // Fallback to default description if no attribute found
            return $"Icon: {iconName}";
        }

        return $"Icon: {identifier}";
    }

    private static FieldInfo? FindFieldWithIconName(string iconName)
    {
        // Get all nested types from OverlayIcons
        var nestedTypes = typeof(OverlayIcons).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var type in nestedTypes)
        {
            // Get all public static fields (constants) from this type
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string) && field.GetValue(null) is string fieldValue && fieldValue == iconName)
                {
                    return field;
                }
            }
        }

        return null;
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


