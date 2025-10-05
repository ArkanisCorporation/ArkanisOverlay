namespace Arkanis.Overlay.External.UEX;

using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

internal abstract class UexApiClientBase
{
    private const string UserTokenHeaderName = "secret_key";
    private static readonly UexApiOptions DefaultOptions = new();

    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly IOptionsMonitor<UexApiOptions>? _options;

    protected UexApiClientBase()
    {
    }

    protected UexApiClientBase(IHttpClientFactory httpClientFactory, IOptionsMonitor<UexApiOptions>? options = null) : this()
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public ValueTask<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken = default)
    {
        var options = _options?.CurrentValue ?? DefaultOptions;
        var httpClient = _httpClientFactory?.CreateClient(GetType().Name);
        httpClient ??= new HttpClient();
        httpClient.Timeout = options.Timeout;

        if (options.ApplicationToken is { Length: > 0 } applicationToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", applicationToken);
        }

        if (options.UserToken is { Length: > 0 } userToken)
        {
            httpClient.DefaultRequestHeaders.Add(UserTokenHeaderName, userToken);
        }

        return ValueTask.FromResult(httpClient);
    }
}
