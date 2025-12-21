namespace Arkanis.Overlay.Components.Extensions;

using MudBlazor;

public static class MudBlazorSizeExtensions
{
    public static int ToPixels(this Size size)
        => size switch
        {
            Size.Small => 32,
            Size.Medium => 40,
            Size.Large => 56,
            _ => 40,
        };
}
