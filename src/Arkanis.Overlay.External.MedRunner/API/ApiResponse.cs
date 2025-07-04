namespace Arkanis.Overlay.External.MedRunner.API;

using System.Diagnostics.CodeAnalysis;
using System.Net;

/// <summary>
///     Represents a generic API response.
/// </summary>
public class ApiResponse<T>
{
    [MemberNotNullWhen(true, nameof(Data))]
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool Success { get; set; }

    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}
