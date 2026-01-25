namespace Arkanis.Overlay.External.Regolith.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Mining location/body in the Star Citizen universe.
/// </summary>
public sealed class RegolithBody
{
    /// <summary>
    /// Display name of the body (e.g., "Aaron Halo", "Hurston", "Aberdeen").
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Type of gravitational well (SYSTEM, BELT, PLANET, CLUSTER, LAGRANGE, SATELLITE).
    /// </summary>
    [JsonPropertyName("wellType")]
    public string WellType { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the body (e.g., "AARON_HALO", "HUR", "ABE").
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Star system containing this body.
    /// </summary>
    [JsonPropertyName("system")]
    public string System { get; set; } = string.Empty;

    /// <summary>
    /// Depth in the celestial hierarchy (0 = system, 1 = planet/belt, 2 = moon/lagrange).
    /// </summary>
    [JsonPropertyName("depth")]
    public int Depth { get; set; }

    /// <summary>
    /// Parent body ID, null for systems.
    /// </summary>
    [JsonPropertyName("parent")]
    public string? Parent { get; set; }

    /// <summary>
    /// List of parent body IDs from system down.
    /// </summary>
    [JsonPropertyName("parents")]
    public List<string> Parents { get; set; } = [];

    /// <summary>
    /// Type of the parent body.
    /// </summary>
    [JsonPropertyName("parentType")]
    public string? ParentType { get; set; }

    /// <summary>
    /// Whether space mining is available at this location.
    /// </summary>
    [JsonPropertyName("isSpace")]
    public bool IsSpace { get; set; }

    /// <summary>
    /// Whether surface mining is available at this location.
    /// </summary>
    [JsonPropertyName("isSurface")]
    public bool IsSurface { get; set; }

    /// <summary>
    /// Whether gems (hand-mineable) can be found here.
    /// </summary>
    [JsonPropertyName("hasGems")]
    public bool HasGems { get; set; }

    /// <summary>
    /// Whether rocks (ship/vehicle mineable) can be found here.
    /// </summary>
    [JsonPropertyName("hasRocks")]
    public bool HasRocks { get; set; }
}

/// <summary>
/// Types of gravitational wells/locations.
/// </summary>
public static class RegolithWellType
{
    public const string System = "SYSTEM";
    public const string Belt = "BELT";
    public const string Planet = "PLANET";
    public const string Cluster = "CLUSTER";
    public const string Lagrange = "LAGRANGE";
    public const string Satellite = "SATELLITE";
}
