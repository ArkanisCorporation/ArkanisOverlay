namespace Arkanis.Overlay.UI.Models;

using Avalonia.Media;
using Material.Icons;

public abstract record Icon
{
    public IBrush? Color { get; init; }
}

public abstract record MaterialSymbol : Icon
{
    public string? Text { get; init; }

    public sealed record Library(MaterialIconKind Kind) : MaterialSymbol;
}
