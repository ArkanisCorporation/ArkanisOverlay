namespace Arkanis.Overlay.External.MedRunner.API.Mocks.Endpoints;

using Abstractions;
using Abstractions.Endpoints;
using Models;

public class MockCodeEndpoint(IMedRunnerTokenProvider tokenProvider) : MockApiEndpoint(tokenProvider), ICodeEndpoint
{
    public Task<ApiResponse<List<PromotionalCode>>> GetRedeemedCodesAsync()
        => Task.FromResult(NotSupportedResponse<List<PromotionalCode>>(nameof(MockCodeEndpoint), nameof(GetRedeemedCodesAsync)));

    public Task<ApiResponse<string>> RedeemAsync(string code)
        => Task.FromResult(NotSupportedResponse<string>(nameof(MockCodeEndpoint), nameof(RedeemAsync)));
}
