namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models.Search;

public interface ISearchService
{
    IAsyncEnumerable<string> GetSearchTokensAsync(CancellationToken cancellationToken = default);

    Task<GameEntitySearchResults> SearchAsync(IEnumerable<SearchQuery> queries, CancellationToken cancellationToken = default);
}
