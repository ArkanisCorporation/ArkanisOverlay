namespace Arkanis.Overlay.External.MedRunner.API.Endpoints.Code;

using Abstractions;
using Abstractions.Endpoints;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Models;

/// <inheritdoc cref="ICodeEndpoint" />
public class CodeEndpoint(ApiConfig config, IMedRunnerTokenProvider tokenProvider, IMemoryCache cache, ILogger logger)
    : ApiEndpoint(config, tokenProvider, cache, logger), ICodeEndpoint
{
    /// <inheritdoc />
    protected override string Endpoint
        => "code";

    /// <inheritdoc />
    public async Task<ApiResponse<List<PromotionalCode>>> GetRedeemedCodesAsync()
        => await GetRequestAsync<List<PromotionalCode>>("/redeemed");

    /// <inheritdoc />
    public async Task<ApiResponse<string>> RedeemAsync(string code)
        => await PostRequestAsync<string>($"/redeem/{code}");
}
