namespace Arkanis.Overlay.Domain.Models.Search;

using Abstractions;
using Abstractions.Game;
using Enums;

/// <summary>
///     A prototype for any search query.
///     Matching queries against <see cref="ISearchable" /> subjects produces match results.
/// </summary>
public abstract record SearchQuery
{
    public virtual SearchMatchResult<T> Match<T>(T searchable) where T : ISearchable
        => SearchMatchResult.Create(searchable, Match(searchable.SearchableAttributes));

    public virtual SearchMatchResult Match(ISearchable searchable)
        => SearchMatchResult.Create(searchable, Match(searchable.SearchableAttributes));

    public virtual IEnumerable<SearchMatch> Match(IEnumerable<SearchableTrait> attributes, int depth = 0)
        => attributes.SelectMany(attribute => Match(attribute, depth));

    public abstract IEnumerable<SearchMatch> Match(SearchableTrait trait, int depth = 0);
}

public sealed record EmptySearch : SearchQuery
{
    private EmptySearch()
    {
    }

    public static SearchQuery Instance { get; } = new EmptySearch();

    public override IEnumerable<SearchMatch> Match(SearchableTrait trait, int depth = 0)
        => [];
}

public abstract record TextSearch(string Content) : SearchQuery
{
    protected string NormalizedContent { get; } = Content.ToLowerInvariant();

    public static FuzzyTextSearch Combine(IEnumerable<SearchQuery> queries)
        => new(string.Join(' ', queries.OfType<TextSearch>().Select(x => x.Content)));

    public static SearchQuery Fuzzy(string content)
        => FuzzyTextSearch.Create(content);
}

public sealed record FuzzyTextSearch(string Content) : TextSearch(Content)
{
    private static int GetStringComparisonScore(string subject, string query, StringComparison comparison, Func<int>? fallback = null)
        => subject switch
        {
            _ when subject.Equals(query, comparison) => 100,
            _ when subject.StartsWith(query, comparison) => 99,
            _ when subject.EndsWith(query, comparison) => 98,
            _ when subject.Contains(query, comparison) => 97,
            _ => fallback?.Invoke() ?? 0,
        };

    public override IEnumerable<SearchMatch> Match(SearchableTrait trait, int depth = 0)
        => trait switch
        {
            SearchableCode data =>
            [
                //! Location Codes for partial but case-sensitive exact match
                GetStringComparisonScore(data.Code, Content, StringComparison.Ordinal) is > 0 and var score
                    ? new ScoredMatch(score, depth, trait, this)
                    : new NoMatch(trait, this),
            ],
            SearchableName data =>
            [
                GetStringComparisonScore(data.Name, NormalizedContent, StringComparison.OrdinalIgnoreCase) is > 0 and var score
                    ? new ScoredMatch(score, depth, trait, this)
                    : new NoMatch(trait, this),
            ],
            _ => [new NoMatch(trait, this)],
        };

    public static SearchQuery Create(string content)
        => new FuzzyTextSearch(content);
}

public sealed record LocationSearch(IGameLocation Location) : SearchQuery
{
    public override IEnumerable<SearchMatch> Match(SearchableTrait trait, int depth = 0)
        => trait switch
        {
            SearchableLocation data => Location.IsOrContains(data.Location)
                ? [new SoftMatch(trait, this)]
                : [new ExcludeMatch(trait, this)],
            _ => [new NoMatch(trait, this)],
        };
}

public sealed record EntityCategorySearch(GameEntityCategory Category, bool ExcludeOnMismatch = true) : SearchQuery
{
    public override IEnumerable<SearchMatch> Match(SearchableTrait trait, int depth = 0)
        => trait switch
        {
            SearchableEntityCategory data => data.Category == Category
                ? [new SoftMatch(trait, this)]
                : ExcludeOnMismatch
                    ? [new ExcludeMatch(trait, this)]
                    : [new NoMatch(trait, this)],
            _ => [new NoMatch(trait, this)],
        };
}
