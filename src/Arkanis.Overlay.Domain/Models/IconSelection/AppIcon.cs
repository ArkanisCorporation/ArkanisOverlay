namespace Arkanis.Overlay.Domain.Models.IconSelection;

/// <summary>
/// Abstract base record for all application icons.
/// Provides a common interface for different types of icons used in the application.
/// </summary>
public abstract record AppIcon
{
    /// <summary>
    /// The unique identifier for this icon.
    /// </summary>
    public required string Identifier { get; init; }

    /// <summary>
    /// Human-readable description of the icon usage.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The category this icon belongs to.
    /// </summary>
    public required IconCategory Category { get; init; }

    /// <summary>
    /// Whether this icon is currently active/enabled.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Priority for fallback selection (higher values preferred).
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Alternative icon identifiers if the primary is unavailable.
    /// </summary>
    public IReadOnlyList<string> FallbackIcons { get; init; } = [];

    /// <summary>
    /// Converts this AppIcon to an IconSelectionObject for compatibility.
    /// </summary>
    public abstract IconSelectionObject ToIconSelectionObject();
}

/// <summary>
/// Represents a MudBlazor icon with specific variant and style information.
/// </summary>
public sealed record MudIcon : AppIcon
{
    /// <summary>
    /// The MudBlazor icon variant (Filled, Outlined, Rounded, etc.).
    /// </summary>
    public required string Variant { get; init; } = "Filled";

    /// <summary>
    /// The specific icon name within the MudBlazor icon set.
    /// </summary>
    public required string IconName { get; init; }

    /// <summary>
    /// The icon type (MaterialIcons, MaterialSymbols, etc.).
    /// </summary>
    public required IconType Type { get; init; }

    public override IconSelectionObject ToIconSelectionObject()
        => new()
        {
            Category = Category,
            Type = Type,
            IconName = IconName,
            Description = Description,
            IsActive = IsActive,
            Priority = Priority,
            FallbackIcons = FallbackIcons
        };
}

/// <summary>
/// Represents a Material Design icon with specific variant information.
/// </summary>
public sealed record MaterialIcon : AppIcon
{
    /// <summary>
    /// The Material Design icon variant (Filled, Outlined, Rounded, etc.).
    /// </summary>
    public required string Variant { get; init; } = "Filled";

    /// <summary>
    /// The specific icon name within the Material Design icon set.
    /// </summary>
    public required string IconName { get; init; }

    public override IconSelectionObject ToIconSelectionObject()
        => new()
        {
            Category = Category,
            Type = IconType.MaterialIcons,
            IconName = IconName,
            Description = Description,
            IsActive = IsActive,
            Priority = Priority,
            FallbackIcons = FallbackIcons
        };
}

/// <summary>
/// Represents a Material Symbols icon with specific variant information.
/// </summary>
public sealed record MaterialSymbolIcon : AppIcon
{
    /// <summary>
    /// The Material Symbols icon variant (Outlined, Rounded, Sharp, etc.).
    /// </summary>
    public required string Variant { get; init; } = "Outlined";

    /// <summary>
    /// The specific icon name within the Material Symbols icon set.
    /// </summary>
    public required string IconName { get; init; }

    public override IconSelectionObject ToIconSelectionObject()
        => new()
        {
            Category = Category,
            Type = IconType.MaterialSymbols,
            IconName = IconName,
            Description = Description,
            IsActive = IsActive,
            Priority = Priority,
            FallbackIcons = FallbackIcons
        };
}

/// <summary>
/// Represents a custom icon with specific implementation details.
/// </summary>
public sealed record CustomIcon : AppIcon
{
    /// <summary>
    /// The custom icon implementation details (e.g., SVG path, font class, etc.).
    /// </summary>
    public required string Implementation { get; init; }

    /// <summary>
    /// The custom icon format (SVG, Font, Image, etc.).
    /// </summary>
    public required string Format { get; init; }

    public override IconSelectionObject ToIconSelectionObject()
        => new()
        {
            Category = Category,
            Type = IconType.Custom,
            IconName = Identifier,
            Description = Description,
            IsActive = IsActive,
            Priority = Priority,
            FallbackIcons = FallbackIcons
        };
}

