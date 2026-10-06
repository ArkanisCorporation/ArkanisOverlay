namespace Arkanis.Overlay.External.UEX;

using System.ComponentModel.Design;
using System.Net.Http.Headers;
using Abstractions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

internal abstract class UexApiClientBase : IUexApiClient
{
    private const string UserTokenHeaderName = "secret_key";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<UexApiOptions> _options;

    [UsedImplicitly]
    protected UexApiClientBase()
    {
    }

    protected UexApiClientBase(IHttpClientFactory httpClientFactory, IOptionsMonitor<UexApiOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public UexApiOptions CurrentOptions
        => _options.CurrentValue;

    public UexApiOptions? OverrideOptions { get; set; }

    [UsedImplicitly]
    public ValueTask<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(GetType().Name);

        var options = OverrideOptions ?? _options.CurrentValue;
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

    public TClient CloneAs<TClient>() where TClient : class, IUexApiClient
        => ActivatorUtilities.CreateInstance(new ServiceContainer(), GetType(), _httpClientFactory, _options) as TClient
           ?? throw new InvalidOperationException($"Could not clone {GetType()} as {typeof(TClient)}.");
}
