namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using System.Text.Json;
using Abstractions.Endpoints;
using Models;

public class MockClientEndpoint : MockEndpointBase, IClientEndpoint
{
    public Person Person { get; set; } = new()
    {
        Active = true,
        DiscordId = "224580858432978944",
        ClientStats = new ClientStats
        {
            Missions = new EmergencyStats(),
        },
        ClientPortalPreferences = JsonDocument.Parse("{}"),
        Id = "330e45a6-94b5-499f-96ee-00d32f6db404",
    };

    public ClientBlockedStatus BlockedStatus { get; set; } = new();

    public List<ClientHistory> History { get; set; } =
    [
        new()
        {
            EmergencyId = "f7d95029-8bd5-44d9-a400-34b0f41ce209",
            ClientId = "330e45a6-94b5-499f-96ee-00d32f6db404",
            EmergencyCreationTimestamp = DateTimeOffset.UtcNow,
        },
    ];

    public Task<ApiResponse<Person>> GetAsync()
        => OkResponseAsync(Person);

    public Task<ApiResponse<ApiPaginatedResponse<ClientHistory>>> GetHistoryAsync(int limit, string? paginationToken = null)
        => OkPaginatedResponseAsync(History);

    public Task<ApiResponse<ClientBlockedStatus>> GetBlockedStatusAsync()
        => OkResponseAsync(BlockedStatus);

    public Task<ApiResponse<Person>> LinkClientAsync(string rsiHandle)
        => Task.FromResult(NotSupportedResponse<Person>(nameof(MockClientEndpoint), nameof(LinkClientAsync)));

    public Task<ApiResponse<string>> SetUserSettingsAsync(string settings)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockClientEndpoint), nameof(SetUserSettingsAsync)));

    public Task<ApiResponse<string>> DeactivateAsync()
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockClientEndpoint), nameof(DeactivateAsync)));
}
