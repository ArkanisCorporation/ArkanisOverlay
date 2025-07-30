namespace Arkanis.Overlay.External.UEX;

using System.Net.Http.Headers;

internal abstract class UexApiClientBase
{
    private const string UserTokenHeaderName = "secret_key";

    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly UexApiOptions _options;

    protected UexApiClientBase()
    {
    }

    protected UexApiClientBase(IHttpClientFactory httpClientFactory, UexApiOptions? options = null) : this()
    {
        _httpClientFactory = httpClientFactory;
        _options = options ?? new UexApiOptions();
    }

    public ValueTask<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory?.CreateClient(GetType().Name);
        httpClient ??= new HttpClient();
        httpClient.Timeout = _options.Timeout;

        if (_options.ApplicationToken is { Length: > 0 } applicationToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", applicationToken);
        }

        if (_options.UserToken is { Length: > 0 } userToken)
        {
            httpClient.DefaultRequestHeaders.Add(UserTokenHeaderName, userToken);
        }

        return ValueTask.FromResult(httpClient);
    }
}
