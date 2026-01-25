namespace Arkanis.Overlay.External.Regolith.Services;

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Abstractions;
using Microsoft.Extensions.Logging;
using Models;

/// <summary>
/// GraphQL client for the Regolith.Rocks API.
/// </summary>
public sealed class RegolithApiClient : IRegolithApiClient
{
    private const string BaseUrl = "https://api.regolith.rocks";
    private const string ApiKeyHeader = "x-api-key";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<RegolithApiClient> _logger;
    private string? _apiKey;

    public RegolithApiClient(HttpClient httpClient, ILogger<RegolithApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public bool HasApiKey => !string.IsNullOrWhiteSpace(_apiKey);

    public void SetApiKey(string? apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<RegolithLookups?> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        if (!HasApiKey)
        {
            _logger.LogWarning("Regolith API key not configured");
            return null;
        }

        const string query = """
            {
                lookups {
                    CIG {
                        densitiesLookups
                        methodsBonusLookup
                        oreProcessingLookup
                    }
                    UEX {
                        bodies
                        maxPrices
                        refineryBonuses
                        ships
                        tradeports
                    }
                    loadout
                }
            }
            """;

        try
        {
            var response = await ExecuteQueryAsync<RegolithLookupsResponse>(query, cancellationToken);

            if (response?.Errors is { Count: > 0 })
            {
                foreach (var error in response.Errors)
                {
                    _logger.LogError("Regolith API error: {Message}", error.Message);
                }

                return null;
            }

            return response?.Data?.Lookups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Regolith lookups");
            return null;
        }
    }

    public async Task<RegolithSurveyData?> GetSurveyDataAsync(
        string dataName,
        string epoch = "latest",
        CancellationToken cancellationToken = default
    )
    {
        if (!HasApiKey)
        {
            _logger.LogWarning("Regolith API key not configured");
            return null;
        }

        var query = $$"""
            {
                surveyData(dataName: "{{dataName}}", epoch: "{{epoch}}") {
                    data
                    dataName
                    epoch
                    lastUpdated
                }
            }
            """;

        try
        {
            var response = await ExecuteQueryAsync<RegolithSurveyDataResponse>(query, cancellationToken);

            if (response?.Errors is { Count: > 0 })
            {
                foreach (var error in response.Errors)
                {
                    _logger.LogError("Regolith API error: {Message}", error.Message);
                }

                return null;
            }

            return response?.Data?.SurveyData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Regolith survey data for {DataName}", dataName);
            return null;
        }
    }

    private async Task<T?> ExecuteQueryAsync<T>(string query, CancellationToken cancellationToken)
        where T : class
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, string.Empty);
        request.Headers.Add(ApiKeyHeader, _apiKey);

        var payload = new { query };
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Regolith API request failed with status {StatusCode}: {Content}",
                response.StatusCode,
                errorContent
            );
            return null;
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }
}
