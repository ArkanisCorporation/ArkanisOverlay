namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using System.Net;
using Abstractions.Endpoints;
using API.Endpoints.ChatMessage.Request;
using Models;

public class MockChatMessageEndpoint(MockClientEndpoint clientEndpoint, MockWebSocketEventProvider eventProvider) : MockApiEndpoint, IChatMessageEndpoint
{
    public Dictionary<string, List<ChatMessage>> ChatMessages { get; set; } = [];

    public Task<ApiResponse<ChatMessage>> GetMessageAsync(string messageId)
        => ChatMessages.SelectMany(x => x.Value).FirstOrDefault(x => x.Id == messageId) is { } message
            ? OkResponseAsync(message)
            : MessageNotFoundResponseAsync<ChatMessage>(messageId);

    public Task<ApiResponse<ApiPaginatedResponse<ChatMessage>>> GetMessageHistoryAsync(string emergencyId, int limit, string? paginationToken = null)
    {
        var messages = GetOrCreateMessagesFor(emergencyId);
        return OkPaginatedResponseAsync(messages);
    }

    public Task<ApiResponse<ChatMessage>> SendMessageAsync(ChatMessageRequest request)
    {
        var chatMessage = new ChatMessage
        {
            Id = Guid.NewGuid().ToString(),
            EmergencyId = request.EmergencyId,
            SenderId = clientEndpoint.Person.Id,
            Content = request.Contents,
            MessageSentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
        return HandleSendMessageAsync(request, chatMessage);
    }

    public Task<ApiResponse<ChatMessage>> UpdateMessageAsync(string messageId, string contents)
    {
        var message = ChatMessages.Values
            .SelectMany(x => x)
            .FirstOrDefault(x => x.Id == messageId);

        if (message == null)
        {
            return MessageNotFoundResponseAsync<ChatMessage>(messageId);
        }

        message.Content = contents;
        eventProvider.SendChatMessageUpdate(message);

        return OkResponseAsync(message);
    }

    public Task<ApiResponse<string>> DeleteMessageAsync(string messageId)
    {
        foreach (var messages in ChatMessages.Values)
        {
            var message = messages.FirstOrDefault(x => x.Id == messageId);
            if (message == null)
            {
                continue;
            }

            messages.Remove(message);
            return OkResponseAsync(messageId);
        }

        return MessageNotFoundResponseAsync<string>(messageId);
    }

    internal Task<ApiResponse<ChatMessage>> SendInternalMessageAsync(ChatMessageRequest request)
    {
        var chatMessage = new ChatMessage
        {
            Id = Guid.NewGuid().ToString(),
            EmergencyId = request.EmergencyId,
            SenderId = "144e3d04-e80f-40b1-9038-92b60bf27652",
            Content = request.Contents,
            MessageSentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
        return HandleSendMessageAsync(request, chatMessage);
    }

    private Task<ApiResponse<ChatMessage>> HandleSendMessageAsync(ChatMessageRequest request, ChatMessage chatMessage)
    {
        var messages = GetOrCreateMessagesFor(request.EmergencyId);
        messages.Add(chatMessage);
        eventProvider.SendNewChatMessage(chatMessage);

        return OkResponseAsync(chatMessage);
    }

    private List<ChatMessage> GetOrCreateMessagesFor(string emergencyId)
    {
        if (!ChatMessages.TryGetValue(emergencyId, out var messages))
        {
            messages = ChatMessages[emergencyId] = [];
        }

        return messages;
    }

    private static Task<ApiResponse<T>> MessageNotFoundResponseAsync<T>(string id) where T : class
        => ErrorResponseAsync<T>($"Message with ID {id} not found.", HttpStatusCode.NotFound);
}
