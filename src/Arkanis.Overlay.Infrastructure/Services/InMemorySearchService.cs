namespace Arkanis.Overlay.Infrastructure.Services;

using System.Diagnostics;
using System.Threading.Channels;
using Common.Extensions;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using Domain.Models.Search;
using Microsoft.Extensions.Logging;
using MoreLinq;

public class InMemorySearchService(
    IGameEntityAggregateRepository aggregateRepository,
    ILogger<InMemorySearchService> logger
) : ISearchService
{
    public IAsyncEnumerable<string> GetSearchTokensAsync(CancellationToken cancellationToken = default)
    {
        return GetTokensFromEntityNames().Distinct();

        async IAsyncEnumerable<string> GetTokensFromEntityNames()
        {
            var names = aggregateRepository.GetAllAsync(cancellationToken)
                .Select(entity => entity.Name)
                .Select(name => name.MainContent);

            await foreach (var name in names)
            {
                foreach (var nameToken in name.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    yield return nameToken;
                }

                if (name is GameEntityName.NameWithCode nameWithCode)
                {
                    yield return nameWithCode.Code;
                }
            }
        }
    }

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

        var searchMatchChannel = Channel.CreateBounded<SearchMatchResult<IGameEntity>>(100);
        var gameEntityBatches = aggregateRepository.GetAllAsync(cancellationToken).Batch(250, cancellationToken);
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
        };

        var searchProcess = Task.Run<Task>(() => Parallel.ForEachAsync(gameEntityBatches, parallelOptions, PerformSearchOnBatch), cancellationToken)
            .ContinueWith(_ => searchMatchChannel.Writer.Complete(), cancellationToken);

        var matches = await searchMatchChannel.Reader.ReadAllAsync(cancellationToken)
            .OrderByDescending(result => result)
            .ToListAsync(cancellationToken);

        await searchProcess;
        var searchElapsed = stopwatch.Elapsed;
        logger.LogDebug("Search yielded {SearchMatches} results in {SearchLengthMs}ms", matches.Count, searchElapsed.TotalMilliseconds);

        return new GameEntitySearchResults(matches, searchElapsed);

        async ValueTask PerformSearchOnBatch(IGameEntity[] entityBatch, CancellationToken ct)
        {
            var matchResults = entityBatch
                .Select(entity => queries.Select(query => query.Match(entity))
                    .FallbackIfEmpty(SearchMatchResult.CreateEmpty(entity))
                    .Aggregate((result1, result2) => result1.Merge(result2))
                )
                .Where(result => !result.ShouldBeExcluded)
                .Where(result => !result.ContainsUnmatched<LocationSearch>(where => where.Subject is not (IGamePurchasable or IGameSellable or IGameRentable)))
                .Where(result => !result.ContainsUnmatched<TextSearch>());

            foreach (var matchResult in matchResults)
            {
                await searchMatchChannel.Writer.WriteAsync(matchResult, ct);
            }
        }
    }
}
