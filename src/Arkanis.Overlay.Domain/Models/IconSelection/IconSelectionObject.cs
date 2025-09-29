namespace Arkanis.Overlay.Domain.Models.IconSelection;

/// <summary>
/// Represents a complete icon selection with associated metadata.
/// </summary>
public sealed record IconSelectionObject
{
    /// <summary>
    /// The category this icon belongs to.
    /// </summary>
    public required IconCategory Category { get; init; }

    /// <summary>
    /// The source/type of this icon.
    /// </summary>
    public required IconType Type { get; init; }

    /// <summary>
    /// The specific icon identifier/name within the given type.
    /// </summary>
    public required string IconName { get; init; }

    /// <summary>
    /// Human-readable description of the icon usage.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Whether this icon is currently active/enabled.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Priority for fallback selection (higher values preferred).
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Alternative icon names if the primary is unavailable.
    /// </summary>
    public IReadOnlyList<string> FallbackIcons { get; init; } = Array.Empty<string>();
}


