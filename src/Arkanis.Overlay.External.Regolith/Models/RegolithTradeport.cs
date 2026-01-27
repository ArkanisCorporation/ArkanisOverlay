namespace Arkanis.Overlay.External.Regolith.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Tradeport/station with commodity prices.
/// </summary>
public sealed class RegolithTradeport
{
    /// <summary>
    /// Star system (e.g., "STANTON").
    /// </summary>
    [JsonPropertyName("system")]
    public string System { get; set; } = string.Empty;

    /// <summary>
    /// Planet code (e.g., "ARC", "HUR").
    /// </summary>
    [JsonPropertyName("planet")]
    public string Planet { get; set; } = string.Empty;

    /// <summary>
    /// Tradeport code (e.g., "ARCL1", "HURL1").
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the tradeport.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short name for display.
    /// </summary>
    [JsonPropertyName("name_short")]
    public string NameShort { get; set; } = string.Empty;

    /// <summary>
    /// Whether this tradeport has refinery services.
    /// </summary>
    [JsonPropertyName("refinery")]
    public bool Refinery { get; set; }

    /// <summary>
    /// Commodity prices at this tradeport.
    /// </summary>
    [JsonPropertyName("prices")]
    public RegolithTradeportPrices? Prices { get; set; }
}

/// <summary>
/// Commodity prices organized by category.
/// </summary>
public sealed class RegolithTradeportPrices
{
    /// <summary>
    /// Raw ore prices (ore code -> price in aUEC).
    /// </summary>
    [JsonPropertyName("oreRaw")]
    public Dictionary<string, double>? OreRaw { get; set; }

    /// <summary>
    /// Refined ore prices (ore code -> price in aUEC).
    /// </summary>
    [JsonPropertyName("oreRefined")]
    public Dictionary<string, double>? OreRefined { get; set; }

    /// <summary>
    /// Gem prices (gem code -> price in aUEC).
    /// </summary>
    [JsonPropertyName("gems")]
    public Dictionary<string, double>? Gems { get; set; }

    /// <summary>
    /// Salvage material prices.
    /// </summary>
    [JsonPropertyName("salvage")]
    public Dictionary<string, double>? Salvage { get; set; }
}
