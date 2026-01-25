namespace Arkanis.Overlay.External.Regolith.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Root response for surveyData GraphQL query.
/// </summary>
public sealed class RegolithSurveyDataResponse
{
    [JsonPropertyName("data")]
    public RegolithSurveyDataWrapper? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<RegolithGraphQLError>? Errors { get; set; }
}

public sealed class RegolithSurveyDataWrapper
{
    [JsonPropertyName("surveyData")]
    public RegolithSurveyData? SurveyData { get; set; }
}

/// <summary>
/// Survey data containing ore probabilities and statistics.
/// </summary>
public sealed class RegolithSurveyData
{
    /// <summary>
    /// The type of data (e.g., "shipOreByGravProb", "vehicleProbs").
    /// </summary>
    [JsonPropertyName("dataName")]
    public string DataName { get; set; } = string.Empty;

    /// <summary>
    /// The game epoch/patch this data is for.
    /// </summary>
    [JsonPropertyName("epoch")]
    public string Epoch { get; set; } = string.Empty;

    /// <summary>
    /// When this data was last updated (Unix timestamp).
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public long? LastUpdated { get; set; }

    /// <summary>
    /// The actual survey data. Structure varies by dataName.
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

/// <summary>
/// Valid dataName values for surveyData query.
/// </summary>
public static class RegolithSurveyDataName
{
    public const string VehicleProbs = "vehicleProbs";
    public const string ShipOreByGravProb = "shipOreByGravProb";
    public const string ShipOreByRockClassProb = "shipOreByRockClassProb";
    public const string ShipRockClassByGravProb = "shipRockClassByGravProb";
    public const string BonusMap = "bonusMap";
    public const string BonusMapRoc = "bonusMap.roc";
    public const string LeaderBoard = "leaderBoard";
    public const string GuildLeaderBoard = "guildLeaderBoard";
}
