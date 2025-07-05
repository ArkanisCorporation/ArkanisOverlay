namespace Arkanis.Overlay.External.MedRunner.API.Mocks;

using System.Net;

/// <summary>
///     Base class for all mock endpoints that returns non-successful responses.
/// </summary>
public abstract class MockEndpointBase
{
    protected static Task<ApiResponse<T>> OkResponseAsync<T>(T data)
        where T : class
    {
        ApiResponse<T> response = new()
        {
            Success = true,
            Data = data,
            StatusCode = HttpStatusCode.OK,
        };
        return Task.FromResult(response);
    }

    protected static Task<ApiResponse<ApiPaginatedResponse<T>>> OkPaginatedResponseAsync<T>(IEnumerable<T> data)
        where T : class
    {
        ApiResponse<ApiPaginatedResponse<T>> response = new()
        {
            Success = true,
            Data = new ApiPaginatedResponse<T>
            {
                Data = data.ToList(),
                PaginationToken = null,
            },
            StatusCode = HttpStatusCode.OK,
        };
        return Task.FromResult(response);
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
