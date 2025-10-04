namespace Arkanis.Overlay.Domain.Constants;

/// <summary>
/// Centralized icon constants for the Arkanis Overlay application.
/// These constants provide type-safe icon identifiers that can be used throughout the application.
/// </summary>
public static class OverlayIcons
{
    /// <summary>
    /// Navigation and UI-related icons.
    /// </summary>
    public static class Navigation
    {
        public const string Search = $"{nameof(Navigation)}.{nameof(Search)}";
        public const string ChevronLeft = $"{nameof(Navigation)}.{nameof(ChevronLeft)}";
        public const string OpenInBrowser = $"{nameof(Navigation)}.{nameof(OpenInBrowser)}";
        public const string Web = $"{nameof(Navigation)}.{nameof(Web)}";
    }

    /// <summary>
    /// Action-related icons for user interactions.
    /// </summary>
    public static class Actions
    {
        public const string Add = $"{nameof(Actions)}.{nameof(Add)}";
        public const string Remove = $"{nameof(Actions)}.{nameof(Remove)}";
        public const string Calculate = $"{nameof(Actions)}.{nameof(Calculate)}";
        public const string ContentCopy = $"{nameof(Actions)}.{nameof(ContentCopy)}";
        public const string FrameReload = $"{nameof(Actions)}.{nameof(FrameReload)}";
    }

    /// <summary>
    /// System and application-related icons.
    /// </summary>
    public static class System
    {
        public const string Settings = $"{nameof(System)}.{nameof(Settings)}";
        public const string ScreenshotMonitor = $"{nameof(System)}.{nameof(ScreenshotMonitor)}";
        public const string InstallDesktop = $"{nameof(System)}.{nameof(InstallDesktop)}";
        public const string Notifications = $"{nameof(System)}.{nameof(Notifications)}";
        public const string Deblur = $"{nameof(System)}.{nameof(Deblur)}";
    }

    /// <summary>
    /// Game entity-related icons for in-game objects.
    /// </summary>
    public static class GameEntity
    {
        public const string Diamond = $"{nameof(GameEntity)}.{nameof(Diamond)}";
        public const string Rocket = $"{nameof(GameEntity)}.{nameof(Rocket)}";
        public const string LocalShipping = $"{nameof(GameEntity)}.{nameof(LocalShipping)}";
        public const string Category = $"{nameof(GameEntity)}.{nameof(Category)}";
        public const string Topic = $"{nameof(GameEntity)}.{nameof(Topic)}";
        public const string Domain = $"{nameof(GameEntity)}.{nameof(Domain)}";
        public const string Public = $"{nameof(GameEntity)}.{nameof(Public)}";
        public const string Flight = $"{nameof(GameEntity)}.{nameof(Flight)}";
        public const string Store = $"{nameof(GameEntity)}.{nameof(Store)}";
        public const string GarageDoor = $"{nameof(GameEntity)}.{nameof(GarageDoor)}";
    }

    /// <summary>
    /// Trade and commerce-related icons.
    /// </summary>
    public static class Trade
    {
        public const string AddShoppingCart = $"{nameof(Trade)}.{nameof(AddShoppingCart)}";
        public const string RemoveShoppingCart = $"{nameof(Trade)}.{nameof(RemoveShoppingCart)}";
        public const string CarRental = $"{nameof(Trade)}.{nameof(CarRental)}";
        public const string Storefront = $"{nameof(Trade)}.{nameof(Storefront)}";
        public const string Warehouse = $"{nameof(Trade)}.{nameof(Warehouse)}";
    }

    /// <summary>
    /// Location and positioning-related icons.
    /// </summary>
    public static class Location
    {
        public const string LocationOn = $"{nameof(Location)}.{nameof(LocationOn)}";
        public const string LocationOff = $"{nameof(Location)}.{nameof(LocationOff)}";
    }

    /// <summary>
    /// Status and information-related icons.
    /// </summary>
    public static class Status
    {
        public const string Info = $"{nameof(Status)}.{nameof(Info)}";
        public const string Square = $"{nameof(Status)}.{nameof(Square)}";
    }

    /// <summary>
    /// Social and group-related icons.
    /// </summary>
    public static class Social
    {
        public const string Groups = $"{nameof(Social)}.{nameof(Groups)}";
    }
}
