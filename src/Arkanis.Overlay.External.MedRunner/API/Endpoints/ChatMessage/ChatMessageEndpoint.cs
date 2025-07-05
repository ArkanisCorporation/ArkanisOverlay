namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.ChatMessage;

using System.Globalization;
using Abstractions;
using Abstractions.Endpoints;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;
using Request;

/// <inheritdoc cref="IChatMessageEndpoint" />
public class ChatMessageEndpoint(IMedRunnerClientConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger), IChatMessageEndpoint
{
    /// <inheritdoc />
    protected override string Endpoint
        => "chatMessage";

    /// <inheritdoc />
    public async Task<ApiResponse<ChatMessage>> GetMessageAsync(string messageId)
        => await GetRequestAsync<ChatMessage>(messageId);

    /// <inheritdoc />
    public async Task<ApiResponse<ApiPaginatedResponse<ChatMessage>>> GetMessageHistoryAsync(string emergencyId, int limit, string? paginationToken = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["limit"] = limit.ToString(CultureInfo.InvariantCulture),
        };
        if (!string.IsNullOrEmpty(paginationToken))
        {
            queryParams["paginationToken"] = paginationToken;
        }

        return await GetRequestAsync<ApiPaginatedResponse<ChatMessage>>($"/conversation/{emergencyId}", queryParams);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<ChatMessage>> SendMessageAsync(ChatMessageRequest request)
        => await PostRequestAsync<ChatMessage>("", request);

    /// <inheritdoc />
    public async Task<ApiResponse<ChatMessage>> UpdateMessageAsync(string messageId, string contents)
    {
        var payload = new { contents };
        return await PutRequestAsync<ChatMessage>(messageId, payload);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<string>> DeleteMessageAsync(string messageId)
        => await DeleteRequestAsync<string>(messageId);
}
