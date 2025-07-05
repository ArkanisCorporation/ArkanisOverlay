namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.OrgSettings;

using Abstractions;
using Abstractions.Endpoints;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;

/// <inheritdoc cref="IOrgSettingsEndpoint" />
public class OrgSettingsEndpoint(IMedRunnerClientConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger), IOrgSettingsEndpoint
{
    /// <inheritdoc />
    protected override string Endpoint
        => "orgSettings";

    /// <inheritdoc />
    public async Task<ApiResponse<PublicOrgSettings>> GetPublicSettingsAsync()
        => await GetRequestAsync<PublicOrgSettings>("/public");
}
