namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models.IconSelection;

/// <summary>
/// Repository interface for managing and retrieving icon data.
/// </summary>
public interface IIconRepository
{
    /// <summary>
    /// Gets all available icons.
    /// </summary>
    Task<IReadOnlyList<IconSelectionObject>> GetAllIconsAsync();

    /// <summary>
    /// Gets icons by category.
    /// </summary>
    Task<IReadOnlyList<IconSelectionObject>> GetIconsByCategoryAsync(IconCategory category);

    /// <summary>
    /// Gets icons by type.
    /// </summary>
    Task<IReadOnlyList<IconSelectionObject>> GetIconsByTypeAsync(IconType type);

    /// <summary>
    /// Gets icons by category and type.
    /// </summary>
    Task<IReadOnlyList<IconSelectionObject>> GetIconsByCategoryAndTypeAsync(IconCategory category, IconType type);

    /// <summary>
    /// Gets a specific icon by identifier.
    /// </summary>
    Task<IconSelectionObject?> GetIconByIdentifierAsync(string identifier);

    /// <summary>
    /// Searches icons by name or description.
    /// </summary>
    Task<IReadOnlyList<IconSelectionObject>> SearchIconsAsync(string searchTerm);

    /// <summary>
    /// Gets icons with pagination support.
    /// </summary>
    Task<(IReadOnlyList<IconSelectionObject> Icons, int TotalCount)> GetIconsPagedAsync(
        int pageNumber,
        int pageSize,
        IconCategory? category = null,
        IconType? type = null,
        string? searchTerm = null);

    /// <summary>
    /// Gets the total count of available icons.
    /// </summary>
    Task<int> GetIconCountAsync(IconCategory? category = null, IconType? type = null);

    /// <summary>
    /// Gets all available categories.
    /// </summary>
    Task<IReadOnlyList<IconCategory>> GetAvailableCategoriesAsync();

    /// <summary>
    /// Gets all available icon types.
    /// </summary>
    Task<IReadOnlyList<IconType>> GetAvailableIconTypesAsync();
}
