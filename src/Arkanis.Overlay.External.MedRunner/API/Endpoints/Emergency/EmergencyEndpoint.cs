namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Emergency;

using Abstractions;
using Abstractions.Endpoints;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;
using Request;
using Response;

/// <inheritdoc cref="IEmergencyEndpoint" />
public class EmergencyEndpoint(IMedRunnerClientConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger), IEmergencyEndpoint
{
    /// <inheritdoc />
    protected override string Endpoint
        => "emergency";

    /// <inheritdoc />
    public async Task<ApiResponse<Emergency>> GetEmergencyAsync(string emergencyId)
        => await GetRequestAsync<Emergency>($"/{emergencyId}");

    /// <inheritdoc />
    public async Task<ApiResponse<List<Emergency>>> GetEmergenciesAsync(List<string> emergencyIds)
    {
        var query = string.Join("&id=", emergencyIds);
        return await GetRequestAsync<List<Emergency>>($"/bulk?id={query}");
    }

    /// <inheritdoc />
    public async Task<ApiResponse<Emergency>> CreateEmergencyAsync(CreateEmergencyRequest request)
        => await PostRequestAsync<Emergency>("", request);

    /// <inheritdoc />
    public async Task<ApiResponse<string>> CancelEmergencyWithReasonAsync(string emergencyId, CancellationReason reason)
    {
        var payload = new { reason };
        return await PostRequestAsync<string>($"/{emergencyId}/cancelWithReason", payload);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<string>> RateServicesAsync(string emergencyId, ResponseRating rating, string? remarks = null)
    {
        var payload = new
        {
            rating,
            remarks,
        };
        return await PostRequestAsync<string>($"/{emergencyId}/rate/", payload);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<TeamDetailsResponse>> TeamDetailsAsync(string emergencyId)
        => await GetRequestAsync<TeamDetailsResponse>($"/{emergencyId}/teamDetails");
}
