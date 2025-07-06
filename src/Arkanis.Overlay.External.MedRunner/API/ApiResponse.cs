namespace Arkanis.Overlay.External.MedRunner.API;

using System.Diagnostics.CodeAnalysis;
using System.Net;

/// <summary>
///     Represents a generic API response.
/// </summary>
public class ApiResponse<T>
{
    public ApiResponse(Exception exception)
    {
        Success = false;
        ErrorMessage = exception.Message;
        StatusCode = HttpStatusCode.BadRequest;
    }

    public ApiResponse(T? data, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Success = true;
        Data = data;
        StatusCode = statusCode;
    }

    public ApiResponse()
    {
        Success = false;
        StatusCode = HttpStatusCode.BadRequest;
    }

    [MemberNotNullWhen(true, nameof(Data))]
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool Success { get; set; }

    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}
