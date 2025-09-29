namespace Arkanis.Overlay.Infrastructure.Services.IconManagement;

using Arkanis.Overlay.Domain.Abstractions.Services;
using Arkanis.Overlay.Domain.Enums;
using Arkanis.Overlay.Domain.Models.IconSelection;

public sealed class IconService(IIconValidationService validationService, IIconFallbackService fallbackService)
    : IIconService
{
    private const string DefaultIcon = "Icons.Material.Filled.Square";

    public string GetIconString(IconSelectionObject selection)
    {
        if (validationService.IsValid(selection))
        {
            // Return the icon name for the UI layer to resolve
            return GetIconNameForType(selection);
        }

        return fallbackService.GetFallbackIconString(selection);
    }

    public string GetIconFor<T>(T value)
        => value switch
        {
            GameEntityCategory x => GetIconFor(x),
            PriceType x => GetIconFor(x),
            _ => DefaultIcon,
        };

    private static string GetIconNameForType(IconSelectionObject selection)
        => selection.Type switch
        {
            IconType.MaterialSymbols => selection.IconName,
            IconType.MaterialIcons => selection.IconName,
            IconType.Custom => $"custom:{selection.IconName}",
            _ => "Square",
        };

    private static string GetIconFor(PriceType value)
        => value switch
        {
            PriceType.Buy => "AddShoppingCart",
            PriceType.Sell => "RemoveShoppingCart",
            PriceType.Rent => "CarRental",
            _ => "Square",
        };

    private static string GetIconFor(GameEntityCategory value)
        => value switch
        {
            GameEntityCategory.Commodity => "Diamond",
            GameEntityCategory.SpaceShip => "Rocket",
            GameEntityCategory.GroundVehicle => "LocalShipping",
            GameEntityCategory.Item => "Category",
            GameEntityCategory.ProductCategory => "Topic",
            GameEntityCategory.Company => "Domain",
            GameEntityCategory.Location => "Public",
            _ => "Square",
        };
}


