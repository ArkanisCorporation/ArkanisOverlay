namespace Arkanis.Overlay.External.MedRunner.API.Abstractions.Endpoints;

using API.Endpoints.Emergency.Request;
using API.Endpoints.Emergency.Response;
using Models;

/// <summary>
///     Endpoints for interacting with emergencies.
/// </summary>
public interface IEmergencyEndpoint
{
    /// <summary>
    ///     Gets an emergency by id.
    /// </summary>
    /// <param name="id">The id of the emergency to retrieve.</param>
    /// <returns>An API response containing the emergency details.</returns>
    Task<ApiResponse<Emergency>> GetEmergencyAsync(string id);

    /// <summary>
    ///     Bulk fetches emergencies by id.
    /// </summary>
    /// <param name="ids">The list of emergency ids to fetch.</param>
    /// <returns>An API response containing the list of emergencies.</returns>
    Task<ApiResponse<List<Emergency>>> GetEmergenciesAsync(List<string> ids);

    /// <summary>
    ///     Creates a new emergency.
    /// </summary>
    /// <param name="request">The emergency creation request.</param>
    /// <returns>An API response containing the created emergency.</returns>
    Task<ApiResponse<Emergency>> CreateEmergencyAsync(CreateEmergencyRequest request);

    Task<ApiResponse<string>> CancelEmergencyWithReasonAsync(string id, CancellationReason reason);

    /// <summary>
    ///     Allows the client to rate their emergency response.
    /// </summary>
    /// <param name="id">The id of the emergency to rate.</param>
    /// <param name="rating">The rating given by the client.</param>
    /// <param name="remarks">Optional remarks about the rating.</param>
    /// <returns>An API response indicating success or failure.</returns>
    Task<ApiResponse<string>> RateServicesAsync(string id, ResponseRating rating, string? remarks = null);

    /// <summary>
    ///     Fetches additional details about the responding team for an alert.
    /// </summary>
    /// <param name="id">The id of the emergency.</param>
    /// <returns>An API response containing the team details.</returns>
    Task<ApiResponse<TeamDetailsResponse>> TeamDetailsAsync(string id);
}
