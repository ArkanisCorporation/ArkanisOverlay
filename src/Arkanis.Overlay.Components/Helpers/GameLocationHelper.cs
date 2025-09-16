namespace Arkanis.Overlay.Components.Helpers;

using Domain.Abstractions;
using Domain.Abstractions.Game;

public static class GameLocationHelper
{
    public static IGameLocation[] CollectUniqueLocations<T>(IEnumerable<T> locations)
        => locations
            .CollectLocations()
            .FilterAndSort();

    public static IGameLocation[] CollectUniqueLocationsWithParents<T>(IEnumerable<T> locations)
        => locations
            .CollectLocations()
            .CollectParents()
            .FilterAndSort();

    private static IEnumerable<IGameLocation> CollectLocations<T>(this IEnumerable<T> locations)
        => locations
            .Select(item => item switch
            {
                IGameLocatedAt locatedAt => locatedAt.Location,
                IGameLocation location => location,
                _ => null,
            })
            .Where(item => item is not null)!;

    public static IEnumerable<IGameLocation> CollectParents(this IEnumerable<IGameLocation> locations)
        => locations
            .SelectMany<IGameLocation, IGameLocation>(location => [location, ..location.Parents]);

    private static IGameLocation[] FilterAndSort(this IEnumerable<IGameLocation> locations)
        => locations
            .Distinct(IIdentifiable.EqualityComparer.For<IGameLocation>())
            .OrderBy(x => x.Name.MainContent.FullName)
            .ToArray();
}
