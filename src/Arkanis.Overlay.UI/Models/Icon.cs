namespace Arkanis.Overlay.UI.Models;

using Avalonia.Media;
using JetBrains.Annotations;
using Material.Icons;

public abstract record Icon
{
    public IBrush? Color { get; init; }
}

public sealed record MissingIcon : Icon
{
    public static readonly  MissingIcon Instance = new();
}

public abstract record MaterialSymbol : Icon
{
    public string? Text { get; init; }
}

public sealed record MaterialLibrarySymbol(MaterialIconKind Kind) : MaterialSymbol
{
    [UsedImplicitly]
    public MaterialLibrarySymbol() : this(MaterialIconKind.Square)
    {
    }
}
