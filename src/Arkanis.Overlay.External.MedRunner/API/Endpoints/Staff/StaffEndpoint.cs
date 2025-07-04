namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Staff;

using Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Response;

/// <summary>
///     Endpoints for interacting with staff.
/// </summary>
public class StaffEndpoint(ApiConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger)
{
    protected override string Endpoint
        => "staff";

    /// <summary>
    ///     Gets detailed information about medals.
    /// </summary>
    public async Task<ApiResponse<List<MedalInformation>>> MedalsInformationAsync()
        => await GetRequestAsync<List<MedalInformation>>("/meta/medals");
}
