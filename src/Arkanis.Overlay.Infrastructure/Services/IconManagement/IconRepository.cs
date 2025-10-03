namespace Arkanis.Overlay.Infrastructure.Services.IconManagement;

using Arkanis.Overlay.Domain.Abstractions.Services;
using Arkanis.Overlay.Domain.Models.IconSelection;

/// <summary>
/// Repository implementation for managing and retrieving icon data.
/// </summary>
public sealed class IconRepository : IIconRepository
{
    private readonly IIconDiscoveryService _discoveryService;

    public IconRepository(IIconDiscoveryService discoveryService)
    {
        _discoveryService = discoveryService;
    }

    public Task<IReadOnlyList<IconSelectionObject>> GetAllIconsAsync()
    {
        var icons = _discoveryService.GetAllIcons();
        return Task.FromResult(icons);
    }

    public Task<IReadOnlyList<IconSelectionObject>> GetIconsByCategoryAsync(IconCategory category)
    {
        var icons = _discoveryService.GetIconsByCategory(category);
        return Task.FromResult(icons);
    }

    public Task<IReadOnlyList<IconSelectionObject>> GetIconsByTypeAsync(IconType type)
    {
        var icons = _discoveryService.GetIconsByType(type);
        return Task.FromResult(icons);
    }

    public Task<IReadOnlyList<IconSelectionObject>> GetIconsByCategoryAndTypeAsync(IconCategory category, IconType type)
    {
        var icons = _discoveryService.GetIconsByCategoryAndType(category, type);
        return Task.FromResult(icons);
    }

    public Task<IconSelectionObject?> GetIconByIdentifierAsync(string identifier)
    {
        var allIcons = _discoveryService.GetAllIcons();
        var icon = allIcons.FirstOrDefault(i => i.IconName == identifier);
        return Task.FromResult(icon);
    }

    public Task<IReadOnlyList<IconSelectionObject>> SearchIconsAsync(string searchTerm)
    {
        var icons = _discoveryService.SearchIcons(searchTerm);
        return Task.FromResult(icons);
    }

    public Task<(IReadOnlyList<IconSelectionObject> Icons, int TotalCount)> GetIconsPagedAsync(
        int pageNumber, 
        int pageSize, 
        IconCategory? category = null, 
        IconType? type = null, 
        string? searchTerm = null)
    {
        var allIcons = _discoveryService.GetAllIcons().AsEnumerable();

        // Apply filters
        if (category.HasValue)
        {
            allIcons = allIcons.Where(i => i.Category == category.Value);
        }

        if (type.HasValue)
        {
            allIcons = allIcons.Where(i => i.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            allIcons = _discoveryService.SearchIcons(searchTerm).AsEnumerable();
            
            // Re-apply category and type filters to search results
            if (category.HasValue)
            {
                allIcons = allIcons.Where(i => i.Category == category.Value);
            }
            if (type.HasValue)
            {
                allIcons = allIcons.Where(i => i.Type == type.Value);
            }
        }

        var totalCount = allIcons.Count();
        var pagedIcons = allIcons
            .OrderBy(i => i.Category)
            .ThenBy(i => i.IconName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(((IReadOnlyList<IconSelectionObject>)pagedIcons, totalCount));
    }

    public Task<int> GetIconCountAsync(IconCategory? category = null, IconType? type = null)
    {
        var allIcons = _discoveryService.GetAllIcons().AsEnumerable();

        if (category.HasValue)
        {
            allIcons = allIcons.Where(i => i.Category == category.Value);
        }

        if (type.HasValue)
        {
            allIcons = allIcons.Where(i => i.Type == type.Value);
        }

        return Task.FromResult(allIcons.Count());
    }

    public Task<IReadOnlyList<IconCategory>> GetAvailableCategoriesAsync()
    {
        var categories = _discoveryService.GetAllIcons()
            .Select(i => i.Category)
            .Distinct()
            .OrderBy(c => c.ToString())
            .ToList();

        return Task.FromResult((IReadOnlyList<IconCategory>)categories);
    }

    public Task<IReadOnlyList<IconType>> GetAvailableIconTypesAsync()
    {
        var types = _discoveryService.GetAllIcons()
            .Select(i => i.Type)
            .Distinct()
            .OrderBy(t => t.ToString())
            .ToList();

        return Task.FromResult((IReadOnlyList<IconType>)types);
    }
}
