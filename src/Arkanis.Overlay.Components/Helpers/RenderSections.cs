namespace Arkanis.Overlay.Components.Helpers;

public static class RenderSections
{
    public static class AppBar
    {
        public const string AppMenu = $"{nameof(AppBar)}-{nameof(AppMenu)}";
        public const string UserMenu = $"{nameof(AppBar)}-{nameof(UserMenu)}";
    }

    public static class OverlayControls
    {
        public const string TopRight = $"{nameof(OverlayControls)}-{nameof(TopRight)}";
        public const string BottomRight = $"{nameof(OverlayControls)}-{nameof(BottomRight)}";
        public const string TopLeft = $"{nameof(OverlayControls)}-{nameof(TopLeft)}";
        public const string BottomLeft = $"{nameof(OverlayControls)}-{nameof(BottomLeft)}";
    }
}
