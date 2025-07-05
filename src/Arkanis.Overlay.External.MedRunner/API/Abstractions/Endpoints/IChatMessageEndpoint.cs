namespace Arkanis.Overlay.External.MedRunner.API.Abstractions.Endpoints;

using API.Endpoints.ChatMessage.Request;
using Models;

/// <summary>
///     Endpoints for interacting with chat messages.
/// </summary>
public interface IChatMessageEndpoint
{
    /// <summary>
    ///     Fetch a chat message by id.
    /// </summary>
    /// <param name="messageId">The unique identifier of the chat message.</param>
    /// <returns>An API response containing the chat message.</returns>
    Task<ApiResponse<ChatMessage>> GetMessageAsync(string messageId);

    /// <summary>
    ///     Gets the specified amount of chat messages for a given emergency.
    /// </summary>
    /// <param name="emergencyId">The ID of the emergency to get chat history for.</param>
    /// <param name="limit">The maximum number of messages to return.</param>
    /// <param name="paginationToken">Token for pagination (optional).</param>
    /// <returns>A paginated API response containing chat messages.</returns>
    Task<ApiResponse<ApiPaginatedResponse<ChatMessage>>> GetMessageHistoryAsync(string emergencyId, int limit, string? paginationToken = null);

    /// <summary>
    ///     Sends a new chat message.
    /// </summary>
    /// <param name="request">The message request containing the chat message details.</param>
    /// <returns>An API response containing the sent chat message.</returns>
    Task<ApiResponse<ChatMessage>> SendMessageAsync(ChatMessageRequest request);

    /// <summary>
    ///     Update a chat message.
    /// </summary>
    /// <param name="messageId">The ID of the message to update.</param>
    /// <param name="contents">The new contents of the message.</param>
    /// <returns>An API response containing the updated chat message.</returns>
    Task<ApiResponse<ChatMessage>> UpdateMessageAsync(string messageId, string contents);

    /// <summary>
    ///     Delete a chat message.
    /// </summary>
    /// <param name="messageId">The ID of the message to delete.</param>
    /// <returns>An API response indicating success or failure.</returns>
    Task<ApiResponse<string>> DeleteMessageAsync(string messageId);
}
