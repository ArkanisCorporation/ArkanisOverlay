namespace Arkanis.Overlay.Domain.Models.IconSelection;

/// <summary>
/// Represents a paginated result of icon queries.
/// </summary>
public sealed record PagedIconResult
{
    /// <summary>
    /// Gets the icons for the current page.
    /// </summary>
    public required IReadOnlyList<IconSelectionObject> Icons { get; init; }

    /// <summary>
    /// Gets the total count of icons matching the query criteria.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of pages available.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}

