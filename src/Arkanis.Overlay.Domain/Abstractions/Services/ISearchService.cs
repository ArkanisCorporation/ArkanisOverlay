namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models.Search;

public interface ISearchService
{
    Task<GameEntitySearchResults> SearchAsync(IEnumerable<SearchQuery> queries, CancellationToken cancellationToken = default);
}
