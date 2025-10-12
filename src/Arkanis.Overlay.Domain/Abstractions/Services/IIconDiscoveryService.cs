namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models.IconSelection;

/// <summary>
/// Service for discovering and managing available icons.
/// </summary>
public interface IIconDiscoveryService
{
    /// <summary>
    /// Gets all available icons discovered from constants.
    /// </summary>
    public IReadOnlyList<IconSelectionObject> GetAllIcons();

    /// <summary>
    /// Gets icons filtered by category.
    /// </summary>
    public IReadOnlyList<IconSelectionObject> GetIconsByCategory(IconCategory category);

    /// <summary>
    /// Gets icons filtered by type.
    /// </summary>
    public IReadOnlyList<IconSelectionObject> GetIconsByType(IconType type);

    /// <summary>
    /// Gets icons filtered by category and type.
    /// </summary>
    public IReadOnlyList<IconSelectionObject> GetIconsByCategoryAndType(IconCategory category, IconType type);

    /// <summary>
    /// Searches icons by name or description.
    /// </summary>
    public IReadOnlyList<IconSelectionObject> SearchIcons(string searchTerm);
}

