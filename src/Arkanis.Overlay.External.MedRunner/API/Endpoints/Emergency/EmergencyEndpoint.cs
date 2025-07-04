namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Emergency;

using Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;
using Request;
using Response;

/// <summary>
///     Endpoints for interacting with emergencies.
/// </summary>
public class EmergencyEndpoint(ApiConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger)
{
    protected override string Endpoint
        => "emergency";

    /// <summary>
    ///     Gets an emergency by id.
    /// </summary>
    public async Task<ApiResponse<Emergency>> GetEmergencyAsync(string id)
        => await GetRequestAsync<Emergency>($"/{id}");

    /// <summary>
    ///     Bulk fetches emergencies by id.
    /// </summary>
    public async Task<ApiResponse<List<Emergency>>> GetEmergenciesAsync(List<string> ids)
    {
        var query = string.Join("&id=", ids);
        return await GetRequestAsync<List<Emergency>>($"/bulk?id={query}");
    }

    /// <summary>
    ///     Creates a new emergency.
    /// </summary>
    public async Task<ApiResponse<Emergency>> CreateEmergencyAsync(CreateEmergencyRequest newEmergency)
        => await PostRequestAsync<Emergency>("", newEmergency);

    /// <summary>
    ///     Cancels an existing emergency with a reason.
    /// </summary>
    public async Task<ApiResponse<string>> CancelEmergencyWithReasonAsync(string id, CancellationReason reason)
    {
        var payload = new { reason };
        return await PostRequestAsync<string>($"/{id}/cancelWithReason", payload);
    }

    /// <summary>
    ///     Allows the client to rate their emergency.
    /// </summary>
    public async Task<ApiResponse<string>> RateServicesAsync(string id, ResponseRating rating, string? remarks = null)
    {
        var payload = new
        {
            rating,
            remarks,
        };
        return await PostRequestAsync<string>($"/{id}/rate/", payload);
    }

    /// <summary>
    ///     Fetches additional details about the responding team for an alert.
    /// </summary>
    public async Task<ApiResponse<TeamDetailsResponse>> TeamDetailsAsync(string id)
        => await GetRequestAsync<TeamDetailsResponse>($"/{id}/teamDetails");
}
