namespace Arkanis.Overlay.External.MedRunner.API.Endpoints;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Abstractions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

/// <summary>
///     Base class for API endpoints.
/// </summary>
public abstract class ApiEndpoint(
    ApiConfig config,
    IMedRunnerTokenProvider tokenProvider,
    IMemoryCache cache,
    ILogger logger,
    HttpClient? httpClient = null
)
{
    private static readonly Action<ILogger, HttpMethod, string, Exception?> LogRequest = LoggerMessage.Define<HttpMethod, string>(
        LogLevel.Debug,
        default,
        "{Method} {Url}"
    );

    private static readonly Action<ILogger, HttpMethod, string, HttpStatusCode, string, Exception?> LogRequestError =
        LoggerMessage.Define<HttpMethod, string, HttpStatusCode, string>(
            LogLevel.Error,
            default,
            "Received API error response for {Method} {Url}: (code {StatusCode}) {ErrorMessage}"
        );

    private static readonly Action<ILogger, HttpMethod, string, Exception?> LogRequestException =
        LoggerMessage.Define<HttpMethod, string>(LogLevel.Error, default, "An exception occured while handling {Method} {Url}");

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();

    protected ApiConfig Config { get; } = config;

    /// <summary>
    ///     Returns the endpoint path (to be implemented by derived classes).
    /// </summary>
    protected abstract string Endpoint { get; }

    /// <summary>
    ///     Returns the full endpoint URL.
    /// </summary>
    protected string EndpointUrl()
        => $"{Config.BaseUrl.TrimEnd('/')}/{Endpoint.TrimStart('/')}";

    /// <summary>
    ///     Builds the full URL for a request.
    /// </summary>
    protected string BuildUrl(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        var baseUrl = EndpointUrl().TrimEnd('/');
        var url = $"{baseUrl}/{endpoint.TrimStart('/')}";
        return QueryHelpers.AddQueryString(url, queryParams ?? []);
    }

    /// <summary>
    ///     Adds headers for a request, including authentication if needed.
    /// </summary>
    protected virtual async Task<HttpRequestMessage> CreateRequestMessageAsync(
        HttpMethod method,
        string url,
        object? body = null,
        RequestOptions? requestOptions = null
    )
    {
        requestOptions ??= RequestOptions.Default;
        var request = new HttpRequestMessage(method, url);
        if (!requestOptions.IsUnauthenticatedRequest)
        {
            var accessToken = await tokenProvider.GetAccessTokenAsync("API makeRequest");
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    /// <summary>
    ///     Sends a GET request.
    /// </summary>
    protected async Task<ApiResponse<T>> GetRequestAsync<T>(
        string endpoint,
        Dictionary<string, string>? queryParams = null,
        RequestOptions? requestOptions = null
    )
    {
        var url = BuildUrl(endpoint, queryParams);
        var request = await CreateRequestMessageAsync(HttpMethod.Get, url, requestOptions);
        return await SendRequestAsync<T>(request, url, requestOptions);
    }

    /// <summary>
    ///     Sends a POST request.
    /// </summary>
    protected async Task<ApiResponse<T>> PostRequestAsync<T>(string endpoint, object? data = null, RequestOptions? requestOptions = null)
    {
        var url = BuildUrl(endpoint);
        var request = await CreateRequestMessageAsync(HttpMethod.Post, url, data, requestOptions);
        return await SendRequestAsync<T>(request, url, requestOptions);
    }

    /// <summary>
    ///     Sends a PUT request.
    /// </summary>
    protected async Task<ApiResponse<T>> PutRequestAsync<T>(string endpoint, object? data = null, RequestOptions? requestOptions = null)
    {
        var url = BuildUrl(endpoint);
        var request = await CreateRequestMessageAsync(HttpMethod.Put, url, data, requestOptions);
        return await SendRequestAsync<T>(request, url, requestOptions);
    }

    /// <summary>
    ///     Sends a PATCH request.
    /// </summary>
    protected async Task<ApiResponse<T>> PatchRequestAsync<T>(string endpoint, object? data = null, RequestOptions? requestOptions = null)
    {
        var url = BuildUrl(endpoint);
        var request = await CreateRequestMessageAsync(HttpMethod.Patch, url, data, requestOptions);
        return await SendRequestAsync<T>(request, url, requestOptions);
    }

    /// <summary>
    ///     Sends a DELETE request.
    /// </summary>
    protected async Task<ApiResponse<T>> DeleteRequestAsync<T>(
        string endpoint,
        Dictionary<string, string>? queryParams = null,
        RequestOptions? requestOptions = null
    )
    {
        var url = BuildUrl(endpoint, queryParams);
        var request = await CreateRequestMessageAsync(HttpMethod.Delete, url, requestOptions);
        return await SendRequestAsync<T>(request, url, requestOptions);
    }

    /// <summary>
    ///     Sends the HTTP request and handles the response and errors.
    /// </summary>
    private async Task<ApiResponse<T>> SendRequestAsync<T>(HttpRequestMessage request, string url, RequestOptions? requestOptions = null)
    {
        requestOptions ??= RequestOptions.Default;
        ApiResponse<T>? response;

        if (request.Method == HttpMethod.Get)
        {
            response = await cache.GetOrCreateAsync(
                $"{request.Method.Method}-{url}",
                async entry =>
                {
                    entry.SetAbsoluteExpiration(requestOptions.CacheDuration);
                    var responseLive = await SendRequestAsyncCore<T>(request, url);
                    if (!responseLive.Success)
                    {
                        entry.Dispose();
                    }

                    return responseLive;
                }
            );
        }
        else
        {
            response = await SendRequestAsyncCore<T>(request, url);
        }

        response ??= new ApiResponse<T>
        {
            ErrorMessage = "Could not retrieve locally-cached data",
        };

        return response;
    }

    private async Task<ApiResponse<T>> SendRequestAsyncCore<T>(HttpRequestMessage request, string url)
    {
        LogRequest(logger, request.Method, url, null);
        try
        {
            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(content, Options);
                return new ApiResponse<T>
                {
                    Success = true,
                    Data = data,
                };
            }

            LogRequestError(logger, request.Method, url, response.StatusCode, content, null);
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessage = content,
                StatusCode = response.StatusCode,
            };
        }
        catch (Exception ex)
        {
            LogRequestException(logger, request.Method, url, ex);
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }

    protected class RequestOptions
    {
        public static readonly RequestOptions Default = new();

        public static readonly RequestOptions Unauthenticated = new()
        {
            IsUnauthenticatedRequest = true,
        };

        public bool IsUnauthenticatedRequest { get; set; }
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(1);
    }
}
