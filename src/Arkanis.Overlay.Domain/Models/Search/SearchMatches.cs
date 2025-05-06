namespace Arkanis.Overlay.Domain.Models.Search;

/// <summary>
///     Represents a match between a trait and a search query.
///     Each match has its own priority which is used to determine the sort order of the matches.
/// </summary>
/// <param name="TargetTrait">The trait that has been matched by a query</param>
/// <param name="Source">The query that produced the match of a trait</param>
public abstract record SearchMatch(SearchableTrait TargetTrait, SearchQuery Source) : IComparable<SearchMatch>
{
    public abstract int CompareTo(SearchMatch? other);

    public static bool operator <(SearchMatch left, SearchMatch right)
        => ReferenceEquals(left, null)
            ? !ReferenceEquals(right, null)
            : left.CompareTo(right) < 0;

    public static bool operator <=(SearchMatch left, SearchMatch right)
        => ReferenceEquals(left, null) || left.CompareTo(right) <= 0;

    public static bool operator >(SearchMatch left, SearchMatch right)
        => !ReferenceEquals(left, null) && left.CompareTo(right) > 0;

    public static bool operator >=(SearchMatch left, SearchMatch right)
        => ReferenceEquals(left, null)
            ? ReferenceEquals(right, null)
            : left.CompareTo(right) >= 0;
}

/// <summary>
///     Represents an absolute match with a search query.
///     This type of match has the highest priority.
/// </summary>
/// <inheritdoc cref="SearchMatch" />
public sealed record ExactMatch(SearchableTrait TargetTrait, SearchQuery Source) : SearchMatch(TargetTrait, Source)
{
    public override int CompareTo(SearchMatch? other)
        => other switch
        {
            ExactMatch => 0,
            _ => 1,
        };
}

/// <summary>
///     Represents a partial match with a search query.
///     The score further determines its priority within all the other scored matches.
/// </summary>
/// <param name="Score">Score of this particular match (should be normalized to values 0-100)</param>
/// <param name="Depth">Recursive source depth of the trait (relevant for recursive location, or entities with parents)</param>
/// <inheritdoc cref="SearchMatch" />
public sealed record ScoredMatch(int Score, int Depth, SearchableTrait TargetTrait, SearchQuery Source) : SearchMatch(TargetTrait, Source)
{
    public double NormalizedScore { get; init; } = Score / Math.Pow(2, Depth);

    public override int CompareTo(SearchMatch? other)
        => other switch
        {
            ExactMatch => -1,
            ScoredMatch otherScoredMatch => NormalizedScore.CompareTo(otherScoredMatch.NormalizedScore),
            _ => 1,
        };
}

/// <summary>
///     Represents an "inclusion" in the result based on the search query.
/// </summary>
/// <inheritdoc cref="SearchMatch" />
public sealed record SoftMatch(SearchableTrait TargetTrait, SearchQuery Source) : SearchMatch(TargetTrait, Source)
{
    public override int CompareTo(SearchMatch? other)
        => other switch
        {
            ScoredMatch => -1,
            ExactMatch => -1,
            SoftMatch => 0,
            _ => 1,
        };
}

/// <summary>
///     Represents a state where the trait did not match with the search query.
///     This is very common as queries are typically only able to match a subset of all traits.
/// </summary>
/// <inheritdoc cref="SearchMatch" />
public sealed record NoMatch(SearchableTrait TargetTrait, SearchQuery Source) : SearchMatch(TargetTrait, Source)
{
    public override int CompareTo(SearchMatch? other)
        => other switch
        {
            ExcludeMatch => 1,
            NoMatch => 0,
            _ => -1,
        };
}

/// <summary>
///     Represents an "exclusion" from the result based on the search query.
///     This would typically be in the case of an entity type/category mismatch.
/// </summary>
/// <inheritdoc cref="SearchMatch" />
public sealed record ExcludeMatch(SearchableTrait TargetTrait, SearchQuery Source) : SearchMatch(TargetTrait, Source)
{
    public override int CompareTo(SearchMatch? other)
        => other switch
        {
            ExcludeMatch => 0,
            _ => -1,
        };
}
