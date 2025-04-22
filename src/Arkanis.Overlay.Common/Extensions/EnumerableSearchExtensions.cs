namespace Arkanis.Common.Extensions;

using FuzzySharp;

public static class EnumerableSearchExtensions
{
    public const int RequiredRatio = 75;

    public static IEnumerable<T> FuzzySearch<T>(this IEnumerable<T> items, Func<T, string> filterBy, string? searchTerm)
        => items.Select(item => (culture: item, ratio: Fuzz.PartialRatio(filterBy(item).ToLowerInvariant(), searchTerm?.ToLowerInvariant())))
            .Where(result => result.ratio > RequiredRatio)
            .OrderByDescending(result => result.ratio)
            .Select(result => result.culture);
}
