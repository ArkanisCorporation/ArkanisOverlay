namespace Arkanis.Overlay.External.Regolith.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Mining ship information.
/// </summary>
public sealed class RegolithShip
{
    /// <summary>
    /// Ship code/identifier.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Ship name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Cargo capacity in SCU.
    /// </summary>
    [JsonPropertyName("cargo")]
    public int? Cargo { get; set; }

    /// <summary>
    /// Mining capacity in SCU.
    /// </summary>
    [JsonPropertyName("mining")]
    public int? Mining { get; set; }

    /// <summary>
    /// Number of mining laser mounts.
    /// </summary>
    [JsonPropertyName("miningMounts")]
    public int? MiningMounts { get; set; }
}
