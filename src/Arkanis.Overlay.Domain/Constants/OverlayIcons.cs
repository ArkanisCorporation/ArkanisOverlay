namespace Arkanis.Overlay.Domain.Constants;

using Models.IconSelection;

/// <summary>
/// Centralized icon constants for the Arkanis Overlay application.
/// These constants provide type-safe icon identifiers that can be used throughout the application.
/// </summary>
public static class OverlayIcons
{
    /// <summary>
    /// Navigation and UI-related icons.
    /// </summary>
    [IconCategory(IconCategory.Navigation)]
    public static class Navigation
    {
        [IconDescription("Search functionality")]
        public const string Search = $"{nameof(Navigation)}.{nameof(Search)}";

        [IconDescription("Navigate left")]
        public const string ChevronLeft = $"{nameof(Navigation)}.{nameof(ChevronLeft)}";

        [IconDescription("Open in browser")]
        public const string OpenInBrowser = $"{nameof(Navigation)}.{nameof(OpenInBrowser)}";

        [IconDescription("Web or external link")]
        public const string Web = $"{nameof(Navigation)}.{nameof(Web)}";
    }

    /// <summary>
    /// Action-related icons for user interactions.
    /// </summary>
    [IconCategory(IconCategory.Action)]
    public static class Actions
    {
        [IconDescription("Add new item")]
        public const string Add = $"{nameof(Actions)}.{nameof(Add)}";

        [IconDescription("Remove item")]
        public const string Remove = $"{nameof(Actions)}.{nameof(Remove)}";

        [IconDescription("Calculate or compute")]
        public const string Calculate = $"{nameof(Actions)}.{nameof(Calculate)}";

        [IconDescription("Copy to clipboard")]
        public const string ContentCopy = $"{nameof(Actions)}.{nameof(ContentCopy)}";

        [IconDescription("Reload or refresh")]
        public const string FrameReload = $"{nameof(Actions)}.{nameof(FrameReload)}";
    }

    /// <summary>
    /// System and application-related icons.
    /// </summary>
    [IconCategory(IconCategory.System)]
    public static class System
    {
        [IconDescription("Application settings")]
        public const string Settings = $"{nameof(System)}.{nameof(Settings)}";

        [IconDescription("Overlay or monitor")]
        public const string ScreenshotMonitor = $"{nameof(System)}.{nameof(ScreenshotMonitor)}";

        [IconDescription("Install application")]
        public const string InstallDesktop = $"{nameof(System)}.{nameof(InstallDesktop)}";

        [IconDescription("Notifications")]
        public const string Notifications = $"{nameof(System)}.{nameof(Notifications)}";

        [IconDescription("Deblur or focus")]
        public const string Deblur = $"{nameof(System)}.{nameof(Deblur)}";
    }

    /// <summary>
    /// Game entity-related icons for in-game objects.
    /// </summary>
    [IconCategory(IconCategory.GameEntity)]
    public static class GameEntity
    {
        [IconDescription("Commodity or valuable item")]
        public const string Commodity = $"{nameof(GameEntity)}.{nameof(Commodity)}";

        [IconDescription("Space ship")]
        public const string SpaceShip = $"{nameof(GameEntity)}.{nameof(SpaceShip)}";

        [IconDescription("Ground vehicle")]
        public const string GroundVehicle = $"{nameof(GameEntity)}.{nameof(GroundVehicle)}";

        [IconDescription("Item")]
        public const string Item = $"{nameof(GameEntity)}.{nameof(Item)}";

        [IconDescription("Product category")]
        public const string ProductCategory = $"{nameof(GameEntity)}.{nameof(ProductCategory)}";

        [IconDescription("Company or organization")]
        public const string Company = $"{nameof(GameEntity)}.{nameof(Company)}";

        [IconDescription("Location or place")]
        public const string Location = $"{nameof(GameEntity)}.{nameof(Location)}";

        [IconDescription("Flight or travel")]
        public const string Flight = $"{nameof(GameEntity)}.{nameof(Flight)}";

        [IconDescription("Store or shop")]
        public const string Store = $"{nameof(GameEntity)}.{nameof(Store)}";

        [IconDescription("Garage or storage")]
        public const string GarageDoor = $"{nameof(GameEntity)}.{nameof(GarageDoor)}";
    }

    /// <summary>
    /// Trade and commerce-related icons.
    /// </summary>
    [IconCategory(IconCategory.Trade)]
    public static class Trade
    {
        [IconDescription("Buy or purchase")]
        public const string AddShoppingCart = $"{nameof(Trade)}.{nameof(AddShoppingCart)}";

        [IconDescription("Sell or remove")]
        public const string RemoveShoppingCart = $"{nameof(Trade)}.{nameof(RemoveShoppingCart)}";

        [IconDescription("Rent or lease")]
        public const string CarRental = $"{nameof(Trade)}.{nameof(CarRental)}";

        [IconDescription("Storefront or business")]
        public const string Storefront = $"{nameof(Trade)}.{nameof(Storefront)}";

        [IconDescription("Warehouse or storage")]
        public const string Warehouse = $"{nameof(Trade)}.{nameof(Warehouse)}";
    }

    /// <summary>
    /// Location and positioning-related icons.
    /// </summary>
    [IconCategory(IconCategory.System)]
    public static class Location
    {
        [IconDescription("Location enabled")]
        public const string LocationOn = $"{nameof(Location)}.{nameof(LocationOn)}";

        [IconDescription("Location disabled")]
        public const string LocationOff = $"{nameof(Location)}.{nameof(LocationOff)}";
    }

    /// <summary>
    /// Status and information-related icons.
    /// </summary>
    [IconCategory(IconCategory.Status)]
    public static class Status
    {
        [IconDescription("Information")]
        public const string Info = $"{nameof(Status)}.{nameof(Info)}";

        [IconDescription("Default icon")]
        public const string Square = $"{nameof(Status)}.{nameof(Square)}";
    }

    /// <summary>
    /// Social and group-related icons.
    /// </summary>
    [IconCategory(IconCategory.System)]
    public static class Social
    {
        [IconDescription("Groups or teams")]
        public const string Groups = $"{nameof(Social)}.{nameof(Groups)}";
    }
}

