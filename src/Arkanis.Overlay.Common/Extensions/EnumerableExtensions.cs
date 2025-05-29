namespace Arkanis.Overlay.Common.Extensions;

using MoreLinq;

public static class EnumerableExtensions
{
    public static int? IndexOf<T>(this IEnumerable<T> items, T item)
        => items.Index()
            .Where(x => x.Value?.Equals(item) == true)
            .Select(x => x.Key)
            .FirstOrDefault();

    public static int IndexOfOrDefault<T>(this IEnumerable<T> items, T item, int defaultValue = 0)
        => items.Index()
            .Where(x => x.Value?.Equals(item) == true)
            .Select(x => x.Key)
            .FirstOrDefault(defaultValue);
}
