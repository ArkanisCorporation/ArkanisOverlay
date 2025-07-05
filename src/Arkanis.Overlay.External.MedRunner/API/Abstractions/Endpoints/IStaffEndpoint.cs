namespace Arkanis.Overlay.External.MedRunner.API.Abstractions.Endpoints;

using API.Endpoints.Staff.Response;

/// <summary>
///     Endpoints for interacting with staff.
/// </summary>
public interface IStaffEndpoint
{
    /// <summary>
    ///     Gets detailed information about medals.
    /// </summary>
    /// <returns>An API response containing a list of medal information.</returns>
    Task<ApiResponse<List<MedalInformation>>> MedalsInformationAsync();
}
