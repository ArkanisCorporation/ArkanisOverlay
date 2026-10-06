#pragma warning disable CA1716
namespace Arkanis.Overlay.Components.Shared;
#pragma warning restore CA1716

using Overlay.Domain.Models.Game;
using Overlay.External.MedRunner.API.Endpoints.Emergency.Request;
using Overlay.External.MedRunner.Models;

internal static class MedRunnerEmergencyLocation
{
    private const string ClosestLocationPrefix = "Closest location: ";
    private const string LocationTypePrefix = "Location type: ";

    public static IReadOnlyList<string> LocationTypes { get; } =
    [
        "Bunker", "Outpost", "Distribution Center", "Contested Zones", "Orbital Laser Platform",
        "Platform Alignment Facility", "Space", "Surface", "ASD Facility", "QV Station", "Breaker Station", "Other",
    ];

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
        var locationType = path.Reverse().Select(InferLocationType).FirstOrDefault(type => type is not null);

        return new MedRunnerEmergencyLocationSelection(
            new Location
            {
                System = supportedSystem.Name,
                Subsystem = supportedPlanet.Name,
                TertiaryLocation = supportedTertiary.Location?.Name,
            },
            exactLocation,
            locationType,
            locationType is not null
                ? $"{ClosestLocationPrefix}{exactLocation}\n{LocationTypePrefix}{locationType}"
                : $"{ClosestLocationPrefix}{exactLocation}"
        );
    }

    private static string? InferLocationType(GameLocationEntity entity)
        => entity switch
        {
            GamePointOfInterest point => MatchLocationType(point.Subtype) ?? MatchLocationType(point.Type),
            GameOutpost => "Outpost",
            GameSpaceStation => "Space",
            GameCity or GameMoon or GamePlanet => "Surface",
            _ => null,
        };

    private static string? MatchLocationType(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
        {
            return null;
        }

        var normalized = NormalizeType(metadata);
        return LocationTypes.FirstOrDefault(type => string.Equals(NormalizeType(type), normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeType(string value)
        => new(value.Where(char.IsLetterOrDigit).ToArray());

    public static MedRunnerComponentBase.EmergencyDetailsDefaults RestoreDefaults(Emergency emergency)
    {
        var remarks = (emergency.Remarks ?? string.Empty).ReplaceLineEndings("\n").Split('\n');
        var exactLocation = ReadRemark(remarks, ClosestLocationPrefix)
                            ?? string.Join(" / ", new[] { emergency.System, emergency.Subsystem, emergency.TertiaryLocation }
                                .Where(name => !string.IsNullOrWhiteSpace(name)));
        return new MedRunnerComponentBase.EmergencyDetailsDefaults(
            emergency.Id, exactLocation, MatchLocationType(ReadRemark(remarks, LocationTypePrefix)), emergency.Remarks);
    }

    private static string? ReadRemark(IEnumerable<string> remarks, string prefix)
    {
        var line = remarks.Select(value => value.Trim())
            .FirstOrDefault(value => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        return line?[prefix.Length..].Trim() is { Length: > 0 } value ? value : null;
    }

    private static SpaceLocation? FindEnabledLocation(IEnumerable<SpaceLocation> locations, string name)
    {
        var enabled = locations.Where(location => location.Enabled).ToArray();
        return enabled.FirstOrDefault(location => string.Equals(location.Name, name, StringComparison.Ordinal))
               ?? enabled.FirstOrDefault(location => string.Equals(location.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}

internal sealed record MedRunnerEmergencyLocationSelection(Location Location, string ExactLocation, string? LocationType, string? Remarks);
