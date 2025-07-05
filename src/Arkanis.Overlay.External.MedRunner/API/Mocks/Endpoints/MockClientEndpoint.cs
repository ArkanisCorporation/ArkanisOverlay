namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using Abstractions.Endpoints;
using Models;

public class MockClientEndpoint : MockEndpointBase, IClientEndpoint
{
    public Task<ApiResponse<Person>> GetAsync()
        => Task.FromResult(NotSupportedResponse<Person>(nameof(MockClientEndpoint), nameof(GetAsync)));

    public Task<ApiResponse<ApiPaginatedResponse<ClientHistory>>> GetHistoryAsync(int limit, string? paginationToken = null)
        => Task.FromResult(NotSupportedPaginatedResponse<ClientHistory>(nameof(MockClientEndpoint), nameof(GetHistoryAsync)));

    public Task<ApiResponse<ClientBlockedStatus>> GetBlockedStatusAsync()
        => Task.FromResult(NotSupportedResponse<ClientBlockedStatus>(nameof(MockClientEndpoint), nameof(GetBlockedStatusAsync)));

    public Task<ApiResponse<Person>> LinkClientAsync(string rsiHandle)
        => Task.FromResult(NotSupportedResponse<Person>(nameof(MockClientEndpoint), nameof(LinkClientAsync)));

    public Task<ApiResponse<string>> SetUserSettingsAsync(string settings)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockClientEndpoint), nameof(SetUserSettingsAsync)));

    public Task<ApiResponse<string>> DeactivateAsync()
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockClientEndpoint), nameof(DeactivateAsync)));
}
