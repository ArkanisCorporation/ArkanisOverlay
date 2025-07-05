namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Client;

using System.Globalization;
using Abstractions;
using Abstractions.Endpoints;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;

/// <inheritdoc cref="IClientEndpoint" />
public class ClientEndpoint(IMedRunnerClientConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger), IClientEndpoint
{
    /// <inheritdoc />
    protected override string Endpoint
        => "client";

    /// <inheritdoc />
    public async Task<ApiResponse<Person>> GetAsync()
        => await GetRequestAsync<Person>("");

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<ApiResponse<Person>> LinkClientAsync(string rsiHandle)
    {
        var payload = new { rsiHandle };
        return await PostRequestAsync<Person>("/link", payload);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<string>> SetUserSettingsAsync(string settings)
    {
        var payload = new { settingsBlob = settings };
        return await PutRequestAsync<string>("/settings/clientPortal", payload);
    }

    /// <inheritdoc />
    public async Task<ApiResponse<string>> DeactivateAsync()
        => await DeleteRequestAsync<string>("");

    /// <inheritdoc />
    public async Task<ApiResponse<ClientBlockedStatus>> GetBlockedStatusAsync()
        => await GetRequestAsync<ClientBlockedStatus>("/blocked");
}
