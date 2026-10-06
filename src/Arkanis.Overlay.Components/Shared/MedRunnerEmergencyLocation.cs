#pragma warning disable CA1716
namespace Arkanis.Overlay.Components.Shared;
#pragma warning restore CA1716

using Overlay.Domain.Models.Game;
using Overlay.External.MedRunner.API.Endpoints.Emergency.Request;
using Overlay.External.MedRunner.Models;

internal static class MedRunnerEmergencyLocation
{
    public static bool SupportsSystem(GameStarSystem system, LocationSettings settings)
        => FindEnabledLocation(settings.Locations, system.Name.MainContent.FullName) is not null;

    public static MedRunnerEmergencyLocationSelection? Resolve(GameLocationEntity location, LocationSettings settings)
    {
        var path = location.CreatePathToRoot().ToArray();
        var system = path.OfType<GameStarSystem>().FirstOrDefault();
        var planet = path.OfType<GamePlanet>().FirstOrDefault();
        if (system is null || planet is null
            || FindEnabledLocation(settings.Locations, system.Name.MainContent.FullName) is not { } supportedSystem
            || FindEnabledLocation(supportedSystem.Children, planet.Name.MainContent.FullName) is not { } supportedPlanet)
        {
            return null;
        }

        var detailPath = path.SkipWhile(entity => entity != planet).Skip(1).ToArray();
        var supportedTertiary = detailPath.Reverse()
            .Select(entity => (Entity: entity, Location: FindEnabledLocation(supportedPlanet.Children, entity.Name.MainContent.FullName)))
            .FirstOrDefault(match => match.Location is not null);
        var exactLocation = string.Join(" / ", path.Select(entity => entity.Name.MainContent.FullName));
        var hasAdditionalDetails = path.Any(entity => entity != system && entity != planet && entity != supportedTertiary.Entity);
        var locationType = path.Reverse().Select(entity => entity switch
        {
            GameOutpost => "Outpost",
            GameSpaceStation => "Space",
            GameCity => "Surface",
            _ => null,
        }).FirstOrDefault(type => type is not null);

        return new MedRunnerEmergencyLocationSelection(
            new Location
            {
                System = supportedSystem.Name,
                Subsystem = supportedPlanet.Name,
                TertiaryLocation = supportedTertiary.Location?.Name,
            },
            exactLocation,
            locationType,
            hasAdditionalDetails ? $"Closest location: {exactLocation}" : null
        );
    }

    private static SpaceLocation? FindEnabledLocation(IEnumerable<SpaceLocation> locations, string name)
    {
        var enabled = locations.Where(location => location.Enabled).ToArray();
        return enabled.FirstOrDefault(location => string.Equals(location.Name, name, StringComparison.Ordinal))
               ?? enabled.FirstOrDefault(location => string.Equals(location.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}

internal sealed record MedRunnerEmergencyLocationSelection(Location Location, string ExactLocation, string? LocationType, string? Remarks);
