namespace Arkanis.Overlay.Domain.Abstractions.Services;

using Models.Search;

public interface ISearchService
{
    public IAsyncEnumerable<string> GetSearchTokensAsync(CancellationToken cancellationToken = default);

    public Task<GameEntitySearchResults> SearchAsync(IEnumerable<SearchQuery> queries, CancellationToken cancellationToken = default);
}
