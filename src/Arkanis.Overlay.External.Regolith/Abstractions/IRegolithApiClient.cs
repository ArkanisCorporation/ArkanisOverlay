namespace Arkanis.Overlay.External.Regolith.Abstractions;

using Models;

/// <summary>
/// Client for the Regolith.Rocks GraphQL API.
/// </summary>
public interface IRegolithApiClient
{
    /// <summary>
    /// Gets whether the client has a valid API key configured.
    /// </summary>
    bool HasApiKey { get; }

    /// <summary>
    /// Fetches lookup data including CIG game data, UEX prices, and loadout information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The lookups data or null if the request failed.</returns>
    Task<RegolithLookups?> GetLookupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches survey data for a specific data type.
    /// </summary>
    /// <param name="dataName">The type of survey data to fetch (see <see cref="RegolithSurveyDataName"/>).</param>
    /// <param name="epoch">The game epoch/patch version (use "latest" for most recent).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The survey data or null if the request failed.</returns>
    Task<RegolithSurveyData?> GetSurveyDataAsync(
        string dataName,
        string epoch = "latest",
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Updates the API key used for authentication.
    /// </summary>
    /// <param name="apiKey">The new API key, or null to clear.</param>
    void SetApiKey(string? apiKey);
}
