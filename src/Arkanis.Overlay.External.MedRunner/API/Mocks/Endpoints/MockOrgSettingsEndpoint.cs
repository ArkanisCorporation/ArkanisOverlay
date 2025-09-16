namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using Abstractions;
using Abstractions.Endpoints;
using Models;

public class MockOrgSettingsEndpoint(IMedRunnerTokenProvider tokenProvider) : MockApiEndpoint(tokenProvider), IOrgSettingsEndpoint
{
    public PublicOrgSettings PublicSettings { get; set; } = new()
    {
        Status = ServiceStatus.SlightlyDegraded,
        EmergenciesEnabled = true,
        AnonymousAlertsEnabled = false,
        RegistrationEnabled = false,
        MessageOfTheDay = new MessageOfTheDay
        {
            Message = "Welcome, this is a mock org settings endpoint.",
        },
        LocationSettings = new LocationSettings
        {
            Locations = [],
        },
    };

    public Task<ApiResponse<PublicOrgSettings>> GetPublicSettingsAsync()
        => OkResponseAsync(PublicSettings);
}
