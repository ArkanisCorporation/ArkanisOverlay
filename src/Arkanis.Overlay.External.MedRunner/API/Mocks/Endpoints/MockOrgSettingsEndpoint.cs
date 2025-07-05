namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using Abstractions.Endpoints;
using Models;

public class MockOrgSettingsEndpoint : MockEndpointBase, IOrgSettingsEndpoint
{
    public Task<ApiResponse<PublicOrgSettings>> GetPublicSettingsAsync()
        => Task.FromResult(NotSupportedResponse<PublicOrgSettings>(nameof(MockOrgSettingsEndpoint), nameof(GetPublicSettingsAsync)));
}
