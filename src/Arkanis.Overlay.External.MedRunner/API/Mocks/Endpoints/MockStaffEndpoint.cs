namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using Abstractions.Endpoints;
using API.Endpoints.Staff.Response;

public class MockStaffEndpoint : MockEndpointBase, IStaffEndpoint
{
    public Task<ApiResponse<List<MedalInformation>>> MedalsInformationAsync()
        => Task.FromResult(NotSupportedResponse<List<MedalInformation>>(nameof(MockStaffEndpoint), nameof(MedalsInformationAsync)));
}
