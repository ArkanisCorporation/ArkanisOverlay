namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using Abstractions.Endpoints;
using Models;

public class MockChatMessageEndpoint : MockEndpointBase, IChatMessageEndpoint
{
    public Task<ApiResponse<ChatMessage>> GetMessageAsync(string id)
        => Task.FromResult(NotSupportedResponse<ChatMessage>(nameof(MockChatMessageEndpoint), nameof(GetMessageAsync)));

    public Task<ApiResponse<ApiPaginatedResponse<ChatMessage>>> GetMessageHistoryAsync(string emergencyId, int limit, string? paginationToken = null)
        => Task.FromResult(NotSupportedPaginatedResponse<ChatMessage>(nameof(MockChatMessageEndpoint), nameof(GetMessageHistoryAsync)));

    public Task<ApiResponse<ChatMessage>> SendMessageAsync(object message)
        => Task.FromResult(NotSupportedResponse<ChatMessage>(nameof(MockChatMessageEndpoint), nameof(SendMessageAsync)));

    public Task<ApiResponse<ChatMessage>> UpdateMessageAsync(string id, string contents)
        => Task.FromResult(NotSupportedResponse<ChatMessage>(nameof(MockChatMessageEndpoint), nameof(UpdateMessageAsync)));

    public Task<ApiResponse<string>> DeleteMessageAsync(string id)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockChatMessageEndpoint), nameof(DeleteMessageAsync)));
}
