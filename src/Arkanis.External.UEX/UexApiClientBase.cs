namespace Arkanis.External.UEX;

internal abstract class UexApiClientBase
{
    private readonly IHttpClientFactory? _httpClientFactory;

    protected UexApiClientBase()
    {
    }

    protected UexApiClientBase(IHttpClientFactory httpClientFactory) : this()
        => _httpClientFactory = httpClientFactory;

    public ValueTask<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory?.CreateClient(GetType().Name);
        httpClient ??= new HttpClient();

        return ValueTask.FromResult(httpClient);
    }
}
