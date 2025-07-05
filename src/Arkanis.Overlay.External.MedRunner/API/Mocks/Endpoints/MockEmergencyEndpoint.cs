namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using Abstractions.Endpoints;
using API.Endpoints.Emergency.Request;
using API.Endpoints.Emergency.Response;
using Models;

public class MockEmergencyEndpoint : MockEndpointBase, IEmergencyEndpoint
{
    public Task<ApiResponse<Emergency>> GetEmergencyAsync(string id)
        => Task.FromResult(NotSupportedResponse<Emergency>(nameof(MockEmergencyEndpoint), nameof(GetEmergencyAsync)));

    public Task<ApiResponse<List<Emergency>>> GetEmergenciesAsync(List<string> ids)
        => Task.FromResult(NotSupportedResponse<List<Emergency>>(nameof(MockEmergencyEndpoint), nameof(GetEmergenciesAsync)));

    public Task<ApiResponse<Emergency>> CreateEmergencyAsync(CreateEmergencyRequest request)
        => Task.FromResult(NotSupportedResponse<Emergency>(nameof(MockEmergencyEndpoint), nameof(CreateEmergencyAsync)));

    public Task<ApiResponse<string>> CancelEmergencyWithReasonAsync(string id, CancellationReason reason)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockEmergencyEndpoint), nameof(CancelEmergencyWithReasonAsync)));

    public Task<ApiResponse<string>> RateServicesAsync(string id, ResponseRating rating, string? remarks = null)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockEmergencyEndpoint), nameof(RateServicesAsync)));

    public Task<ApiResponse<TeamDetailsResponse>> TeamDetailsAsync(string id)
        => Task.FromResult(NotSupportedResponse<TeamDetailsResponse>(nameof(MockEmergencyEndpoint), nameof(TeamDetailsAsync)));
}
