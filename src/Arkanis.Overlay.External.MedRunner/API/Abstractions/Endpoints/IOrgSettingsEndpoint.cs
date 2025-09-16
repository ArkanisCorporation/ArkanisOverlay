namespace Arkanis.Overlay.External.MedRunner.API.Abstractions.Endpoints;

using Models;

/// <summary>
///     Endpoints for interacting with the public org settings.
/// </summary>
public interface IOrgSettingsEndpoint
{
    /// <summary>
    ///     Get the public org settings.
    /// </summary>
    /// <returns>An API response containing the public organization settings.</returns>
    Task<ApiResponse<PublicOrgSettings>> GetPublicSettingsAsync();
}
