namespace Arkanis.Overlay.External.MedRunner.API.Mocks;

using System.Net;
using Abstractions;
using API.Endpoints;

/// <summary>
///     Base class for all mock endpoints that returns non-successful responses.
/// </summary>
public abstract class MockApiEndpoint(IMedRunnerTokenProvider tokenProvider)
{
    private static readonly Random Random = new();

    protected async Task<ApiResponse<T>> OkResponseAsync<T>(T data, ApiEndpoint.RequestOptions? requestOptions = null)
        where T : class
        => await WithAuthAsync(
            requestOptions,
            async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(Random.Next(50, 400)));
                return new ApiResponse<T>
                {
                    Success = true,
                    Data = data,
                    StatusCode = HttpStatusCode.OK,
                };
            }
        );

    protected async Task<ApiResponse<ApiPaginatedResponse<T>>> OkPaginatedResponseAsync<T>(IEnumerable<T> data)
        where T : class
    {
        await Task.Delay(TimeSpan.FromMilliseconds(Random.Next(50, 1_000)));
        return new ApiResponse<ApiPaginatedResponse<T>>
        {
            Success = true,
            Data = new ApiPaginatedResponse<T>
            {
                Data = data.ToList(),
                PaginationToken = null,
            },
            StatusCode = HttpStatusCode.OK,
        };
    }

    private async Task<ApiResponse<T>> WithAuthAsync<T>(ApiEndpoint.RequestOptions? requestOptions, Func<Task<ApiResponse<T>>> getDataAsync) where T : class
    {
        requestOptions ??= ApiEndpoint.RequestOptions.Default;
        if (requestOptions.IsUnauthenticatedRequest)
        {
            return await getDataAsync();
        }

        var accessToken = await tokenProvider.GetAccessTokenAsync("API makeRequest");
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return await ErrorResponseAsync<T>("Access token not found.", HttpStatusCode.Unauthorized);
        }

        return await getDataAsync();
    }

    protected static Task<ApiResponse<T>> ErrorResponseAsync<T>(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        where T : class
    {
        ApiResponse<T> response = new()
        {
            Success = false,
            ErrorMessage = message,
            StatusCode = statusCode,
        };
        return Task.FromResult(response);
    }

    protected static ApiResponse<T> NotSupportedResponse<T>(string endpointName, string methodName)
        where T : class
        => new()
        {
            Success = false,
            ErrorMessage = $"The {endpointName}.{methodName} method is not supported in the mock implementation.",
        };

    protected static ApiResponse<ApiPaginatedResponse<T>> NotSupportedPaginatedResponse<T>(string endpointName, string methodName)
        where T : class
        => new()
        {
            Success = false,
            ErrorMessage = $"The {endpointName}.{methodName} method is not supported in the mock implementation.",
        };
}
