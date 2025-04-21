namespace Arkanis.Overlay.Infrastructure.Services;

using System.Diagnostics;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Search;
using Microsoft.Extensions.Logging;
using MoreLinq;

public class InMemorySearchService(
    IGameEntityAggregateRepository aggregateRepository,
    ILogger<InMemorySearchService> logger
) : ISearchService
{
    public async Task<GameEntitySearchResults> SearchAsync(IEnumerable<SearchQuery> queries, CancellationToken cancellationToken = default)
    {
        queries = queries.ToList();
        if (!queries.Any())
        {
            logger.LogDebug("No search queries provided, returning empty search result");
            return GameEntitySearchResults.Empty;
        }

        var stopwatch = Stopwatch.StartNew();
        logger.LogDebug("Searching all entities for matches with {@SearchQuery}", queries);

        var matches = await aggregateRepository.GetAllAsync(cancellationToken)
            .Select(entity => queries
                .Select(query => query.Match(entity))
                .FallbackIfEmpty(SearchMatchResult.CreateEmpty(entity))
                .Aggregate((result1, result2) => result1.Merge(result2))
            )
            .Where(result => result.ShouldBeExcluded == false)
            .Where(result => !result.ContainsUnmatched<LocationSearch>(where => where.Subject is not (IGamePurchasable or IGameSellable or IGameRentable)))
            .OrderByDescending(result => result)
            .ToListAsync(cancellationToken);

        var searchElapsed = stopwatch.Elapsed;
        logger.LogDebug("Search yielded {SearchMatches} results in {SearchLengthMs}ms", matches.Count, searchElapsed.TotalMilliseconds);

        return new GameEntitySearchResults(matches, searchElapsed);
    }
}
