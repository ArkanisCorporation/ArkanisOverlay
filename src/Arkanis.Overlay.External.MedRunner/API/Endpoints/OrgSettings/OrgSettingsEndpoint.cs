namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.OrgSettings;

using Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;

/// <summary>
///     Endpoints for interacting with the public org settings.
/// </summary>
public class OrgSettingsEndpoint(ApiConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger)
{
    protected override string Endpoint
        => "orgSettings";

    /// <summary>
    ///     Get the public org settings.
    /// </summary>
    public async Task<ApiResponse<PublicOrgSettings>> GetPublicSettingsAsync()
        => await GetRequestAsync<PublicOrgSettings>("/public");
}
