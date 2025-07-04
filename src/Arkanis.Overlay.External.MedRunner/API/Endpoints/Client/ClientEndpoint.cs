namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Client;

using System.Globalization;
using Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;

/// <summary>
///     Endpoints for interacting with clients.
/// </summary>
public class ClientEndpoint(ApiConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger)
{
    protected override string Endpoint
        => "client";

    /// <summary>
    ///     Gets the current client.
    /// </summary>
    public async Task<ApiResponse<Person>> GetAsync()
        => await GetRequestAsync<Person>("");

    /// <summary>
    ///     Gets the specified amount of emergencies the client has created.
    /// </summary>
    public async Task<ApiResponse<ApiPaginatedResponse<ClientHistory>>> GetHistoryAsync(int limit, string? paginationToken = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["limit"] = limit.ToString(CultureInfo.InvariantCulture),
        };
        if (!string.IsNullOrEmpty(paginationToken))
        {
            queryParams["paginationToken"] = paginationToken;
        }

        return await GetRequestAsync<ApiPaginatedResponse<ClientHistory>>("/history", queryParams);
    }

    /// <summary>
    ///     Gets the blocklist status of the current client.
    /// </summary>
    public async Task<ApiResponse<string>> GetBlockedStatusAsync()
        => await GetRequestAsync<string>("/blocked");

    /// <summary>
    ///     Links the current user to a rsiHandle.
    /// </summary>
    public async Task<ApiResponse<Person>> LinkClientAsync(string rsiHandle)
    {
        var payload = new { rsiHandle };
        return await PostRequestAsync<Person>("/link", payload);
    }

    /// <summary>
    ///     Updates the settings of the current user for the Client Portal.
    /// </summary>
    public async Task<ApiResponse<string>> SetUserSettingsAsync(string settings)
    {
        var payload = new { settingsBlob = settings };
        return await PutRequestAsync<string>("/settings/clientPortal", payload);
    }

    /// <summary>
    ///     Deactivate the current client.
    /// </summary>
    public async Task<ApiResponse<string>> DeactivateAsync()
        => await DeleteRequestAsync<string>("");
}
