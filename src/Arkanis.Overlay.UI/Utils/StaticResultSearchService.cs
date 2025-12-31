namespace Arkanis.Overlay.UI.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Abstractions.Game;
using Domain.Abstractions.Services;
using Domain.Models.Game;
using Domain.Models.Search;

public class StaticResultSearchService : ISearchService
{
    public StaticResultSearchService() : this(DefaultResults)
    {
    }

    public StaticResultSearchService(GameEntitySearchResults results)
        => Results = results;

    public GameEntitySearchResults Results { get; set; }

    public static GameEntitySearchResults DefaultResults { get; set; } = new([], TimeSpan.FromMilliseconds(24))
    {
        GameEntities = new List<SearchMatchResult<IGameEntity>>
        {
            new(new GameItem(0, "Item 0", new GameCompany(0, "Company 0", "C0"), GameProductCategory.Unknown), []),
            new(new GameItem(1, "Item 1", new GameCompany(1, "Company 1", "C1"), GameProductCategory.Unknown), []),
            new(new GameItem(2, "Item 2", new GameCompany(2, "Company 2", "C2"), GameProductCategory.Unknown), []),
        },
    };

    public IAsyncEnumerable<string> GetSearchTokensAsync(CancellationToken cancellationToken = default)
        => Array.Empty<string>().ToAsyncEnumerable();

    public Task<GameEntitySearchResults> SearchAsync(IEnumerable<SearchQuery> queries, CancellationToken cancellationToken = default)
        => Task.FromResult(Results);
}
