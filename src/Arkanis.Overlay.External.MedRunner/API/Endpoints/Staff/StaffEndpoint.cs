namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Staff;

using Abstractions;
using Abstractions.Endpoints;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Response;

/// <inheritdoc cref="IStaffEndpoint" />
public class StaffEndpoint(IMedRunnerClientConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger<StaffEndpoint> logger)
    : ApiEndpoint(config, tokenProvider, cache, logger), IStaffEndpoint
{
    /// <inheritdoc />
    protected override string Endpoint
        => "staff";

    /// <inheritdoc />
    public async Task<ApiResponse<List<MedalInformation>>> MedalsInformationAsync()
        => await GetRequestAsync<List<MedalInformation>>("/meta/medals");
}
