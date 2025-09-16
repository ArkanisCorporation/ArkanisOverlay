namespace Arkanis.Overlay.External.MedRunner.API.Abstractions.Endpoints;

using Models;

/// <summary>
///     Endpoints for interacting with clients.
/// </summary>
public interface IClientEndpoint
{
    /// <summary>
    ///     Gets the current client.
    /// </summary>
    /// <returns>An API response containing the client's person details.</returns>
    Task<ApiResponse<Person>> GetAsync();

    /// <summary>
    ///     Gets the specified amount of emergencies the client has created.
    /// </summary>
    /// <param name="limit">The maximum number of history items to return.</param>
    /// <param name="paginationToken">Token for pagination (optional).</param>
    /// <returns>A paginated API response containing the client's emergency history.</returns>
    Task<ApiResponse<ApiPaginatedResponse<ClientHistory>>> GetHistoryAsync(int limit, string? paginationToken = null);

    /// <summary>
    ///     Links the current user to an RSI handle.
    /// </summary>
    /// <param name="rsiHandle">The RSI handle to link to the current user.</param>
    /// <returns>An API response containing the updated person details.</returns>
    Task<ApiResponse<Person>> LinkClientAsync(string rsiHandle);

    /// <summary>
    ///     Updates the settings of the current user for the Client Portal.
    /// </summary>
    /// <param name="settings">The settings to update as a JSON string.</param>
    /// <returns>An API response indicating success or failure.</returns>
    Task<ApiResponse<string>> SetUserSettingsAsync(string settings);

    /// <summary>
    ///     Deactivates the current client.
    /// </summary>
    /// <returns>An API response indicating success or failure.</returns>
    Task<ApiResponse<string>> DeactivateAsync();

    /// <summary>
    ///     Gets the blocked status of the currently linked client.
    /// </summary>
    /// <returns>
    ///     The blocked status of the client.
    /// </returns>
    Task<ApiResponse<ClientBlockedStatus>> GetBlockedStatusAsync();
}
