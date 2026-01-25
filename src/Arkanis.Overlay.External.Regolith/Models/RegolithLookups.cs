namespace Arkanis.Overlay.External.Regolith.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Root response for the lookups GraphQL query.
/// </summary>
public sealed class RegolithLookupsResponse
{
    [JsonPropertyName("data")]
    public RegolithLookupsData? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<RegolithGraphQLError>? Errors { get; set; }
}

public sealed class RegolithLookupsData
{
    [JsonPropertyName("lookups")]
    public RegolithLookups? Lookups { get; set; }
}

/// <summary>
/// Combined lookup data from CIG, UEX and loadout sources.
/// </summary>
public sealed class RegolithLookups
{
    [JsonPropertyName("CIG")]
    public RegolithCigLookups? Cig { get; set; }

    [JsonPropertyName("UEX")]
    public RegolithUexLookups? Uex { get; set; }

    [JsonPropertyName("loadout")]
    public object? Loadout { get; set; }
}

/// <summary>
/// CIG game data lookups.
/// </summary>
public sealed class RegolithCigLookups
{
    /// <summary>
    /// Ore densities by ore code (e.g., "QUANTANIUM" -> 20.45).
    /// </summary>
    [JsonPropertyName("densitiesLookups")]
    public Dictionary<string, double>? DensitiesLookups { get; set; }

    /// <summary>
    /// Refinery method bonuses. Key is method name, value is [yieldBonus, timeMultiplier, costMultiplier].
    /// </summary>
    [JsonPropertyName("methodsBonusLookup")]
    public Dictionary<string, double[]>? MethodsBonusLookup { get; set; }

    /// <summary>
    /// Ore processing data. Key is ore code, value is [baseYield, processMultiplier, priceMultiplier].
    /// </summary>
    [JsonPropertyName("oreProcessingLookup")]
    public Dictionary<string, double[]>? OreProcessingLookup { get; set; }
}

/// <summary>
/// UEX data lookups.
/// </summary>
public sealed class RegolithUexLookups
{
    /// <summary>
    /// Mining locations/bodies in the universe.
    /// </summary>
    [JsonPropertyName("bodies")]
    public List<RegolithBody>? Bodies { get; set; }

    /// <summary>
    /// Maximum prices for commodities, organized by category.
    /// </summary>
    [JsonPropertyName("maxPrices")]
    public RegolithMaxPrices? MaxPrices { get; set; }

    /// <summary>
    /// Refinery bonuses by refinery code. Value is dictionary of ore -> bonus multiplier.
    /// </summary>
    [JsonPropertyName("refineryBonuses")]
    public Dictionary<string, Dictionary<string, double>>? RefineryBonuses { get; set; }

    /// <summary>
    /// Ships data.
    /// </summary>
    [JsonPropertyName("ships")]
    public List<RegolithShip>? Ships { get; set; }

    /// <summary>
    /// Tradeports with prices.
    /// </summary>
    [JsonPropertyName("tradeports")]
    public List<RegolithTradeport>? Tradeports { get; set; }
}

/// <summary>
/// Maximum prices organized by commodity category.
/// </summary>
public sealed class RegolithMaxPrices
{
    /// <summary>
    /// Maximum prices for raw ores.
    /// </summary>
    [JsonPropertyName("oreRaw")]
    public Dictionary<string, RegolithPriceInfo>? OreRaw { get; set; }

    /// <summary>
    /// Maximum prices for refined ores.
    /// </summary>
    [JsonPropertyName("oreRefined")]
    public Dictionary<string, RegolithPriceInfo>? OreRefined { get; set; }

    /// <summary>
    /// Maximum prices for gems.
    /// </summary>
    [JsonPropertyName("gems")]
    public Dictionary<string, RegolithPriceInfo>? Gems { get; set; }
}

/// <summary>
/// Price information for a commodity including min, max, and average prices.
/// </summary>
public sealed class RegolithPriceInfo
{
    /// <summary>
    /// Minimum price data: [price, [locations]].
    /// </summary>
    [JsonPropertyName("min")]
    public JsonElement? Min { get; set; }

    /// <summary>
    /// Maximum price data: [price, [locations]].
    /// </summary>
    [JsonPropertyName("max")]
    public JsonElement? Max { get; set; }

    /// <summary>
    /// Average price.
    /// </summary>
    [JsonPropertyName("avg")]
    public double? Avg { get; set; }

    /// <summary>
    /// Gets the maximum price value, or null if not available.
    /// </summary>
    public double? GetMaxPrice()
    {
        if (Max is not { ValueKind: JsonValueKind.Array } maxElement)
        {
            return null;
        }

        var enumerator = maxElement.EnumerateArray();
        return enumerator.MoveNext() && enumerator.Current.TryGetDouble(out var price) ? price : null;
    }

    /// <summary>
    /// Gets the minimum price value, or null if not available.
    /// </summary>
    public double? GetMinPrice()
    {
        if (Min is not { ValueKind: JsonValueKind.Array } minElement)
        {
            return null;
        }

        var enumerator = minElement.EnumerateArray();
        return enumerator.MoveNext() && enumerator.Current.TryGetDouble(out var price) ? price : null;
    }
}

/// <summary>
/// GraphQL error response.
/// </summary>
public sealed class RegolithGraphQLError
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("locations")]
    public List<RegolithGraphQLErrorLocation>? Locations { get; set; }

    [JsonPropertyName("path")]
    public List<string>? Path { get; set; }
}

public sealed class RegolithGraphQLErrorLocation
{
    [JsonPropertyName("line")]
    public int Line { get; set; }

    [JsonPropertyName("column")]
    public int Column { get; set; }
}
