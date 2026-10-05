namespace Arkanis.Overlay.Infrastructure.UnitTests.CacheDecorators;

using System.Threading;
using System.Threading.Tasks;
using External.UEX.Abstractions;
using Microsoft.Extensions.Logging;

public sealed class UexStaticApiCacheDecorator(IUexStaticApi staticApi, ILogger<UexStaticApiCacheDecorator> logger) : UexApiCacheDecorator(logger),
    IUexStaticApi
{
    public Task<UexApiResponse> GetDataExtractAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetDataExtractAsync),
            _ => staticApi.GetDataExtractAsync(cancellationToken)
        );

    public Task<UexApiResponse<GetDataParametersOkResponse>> GetDataParametersAsync(CancellationToken cancellationToken = default)
        => CacheAsync(
            new { },
            nameof(GetDataParametersAsync),
            _ => staticApi.GetDataParametersAsync(cancellationToken)
        );
}
