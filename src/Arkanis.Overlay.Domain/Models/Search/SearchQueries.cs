namespace Arkanis.Overlay.Domain.Models.Search;

using Abstractions;
using Abstractions.Game;
using Enums;
using FuzzySharp;

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

    public static SearchQuery Fuzzy(string content)
        => new FuzzyTextSearch(content);
}

public sealed record FuzzyTextSearch(string Content) : TextSearch(Content)
{
    public override IEnumerable<SearchMatch> Match(SearchableTrait trait, int depth = 0)
        => trait switch
        {
            SearchableCode data => [new ScoredMatch(Fuzz.Ratio(Content, data.Code), depth, trait, this)],
            SearchableEntityCategory data
                => [new ScoredMatch(Fuzz.Ratio(NormalizedContent, data.Category.ToString("G").ToLowerInvariant()), depth, trait, this)],
            SearchableManufacturer data => Match(data.Manufacturer.SearchableAttributes.OfType<SearchableTextTrait>(), depth + 1),
            SearchableLocation data => Match(data.Location.Parent?.SearchableAttributes ?? [], depth + 1),
            SearchableName data => [new ScoredMatch(Fuzz.TokenSortRatio(Content, data.Name), depth, trait, this)],
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

public sealed record EntityCategorySearch(GameEntityCategory Category) : SearchQuery
{
    public override IEnumerable<SearchMatch> Match(SearchableTrait trait, int depth = 0)
        => trait switch
        {
            SearchableEntityCategory data => data.Category == Category
                ? [new SoftMatch(trait, this)]
                : [new ExcludeMatch(trait, this)],
            _ => [new NoMatch(trait, this)],
        };
}
