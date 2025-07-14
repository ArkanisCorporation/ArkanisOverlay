namespace Arkanis.Overlay.External.MedRunner.API;

/// <summary>
///     Response data which is paginated and includes the data and a token to get the next page.
/// </summary>
public class ApiPaginatedResponse<T>
{
    /// <summary>
    ///     The page of data returned by the request
    /// </summary>
    public required List<T> Data { get; set; }

    /// <summary>
    ///     The pagination token to get the next page of data in a subsequent request
    /// </summary>
    public string? PaginationToken { get; set; }
}
