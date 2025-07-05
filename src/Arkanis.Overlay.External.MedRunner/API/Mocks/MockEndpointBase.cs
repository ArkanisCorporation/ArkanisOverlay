namespace Arkanis.Overlay.External.MedRunner.API.Mocks;

/// <summary>
///     Base class for all mock endpoints that returns non-successful responses.
/// </summary>
public abstract class MockEndpointBase
{
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
