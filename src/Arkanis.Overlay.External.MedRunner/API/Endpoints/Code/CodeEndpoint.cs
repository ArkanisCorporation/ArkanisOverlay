namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Code;

using Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;

/// <summary>
///     Endpoints for interacting with promotional codes.
/// </summary>
public class CodeEndpoint(ApiConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger)
{
    protected override string Endpoint
        => "code";

    /// <summary>
    ///     Gets the redeemed codes for the current user.
    /// </summary>
    public async Task<ApiResponse<List<PromotionalCode>>> GetRedeemedCodesAsync()
        => await GetRequestAsync<List<PromotionalCode>>("/redeemed");

    /// <summary>
    ///     Redeems the specified promotional code for the current user.
    /// </summary>
    public async Task<ApiResponse<string>> RedeemAsync(string code)
        => await PostRequestAsync<string>($"/redeem/{code}");
}
