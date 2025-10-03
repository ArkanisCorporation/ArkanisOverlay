namespace Arkanis.Overlay.External.UEX;

using Microsoft.Extensions.Options;

internal partial class UexGameApi
{
    public UexGameApi(IHttpClientFactory httpClientFactory, IOptions<UexApiOptions> options) : base(httpClientFactory, options)
        => BaseUrl = UexConstants.ApiBaseUrl;
}

internal partial class UexCrewApi
{
    public UexCrewApi(IHttpClientFactory httpClientFactory, IOptions<UexApiOptions> options) : base(httpClientFactory, options)
        => BaseUrl = UexConstants.ApiBaseUrl;
}

internal partial class UexCommoditiesApi
{
    public UexCommoditiesApi(IHttpClientFactory httpClientFactory, IOptions<UexApiOptions> options) : base(httpClientFactory, options)
        => BaseUrl = UexConstants.ApiBaseUrl;
}

internal partial class UexFuelApi
{
    public UexFuelApi(IHttpClientFactory httpClientFactory, IOptions<UexApiOptions> options) : base(httpClientFactory, options)
        => BaseUrl = UexConstants.ApiBaseUrl;
}

internal partial class UexItemsApi
{
    public UexItemsApi(IHttpClientFactory httpClientFactory, IOptions<UexApiOptions> options) : base(httpClientFactory, options)
        => BaseUrl = UexConstants.ApiBaseUrl;
}

internal partial class UexMarketplaceApi
{
    public UexMarketplaceApi(IHttpClientFactory httpClientFactory, IOptions<UexApiOptions> options) : base(httpClientFactory, options)
        => BaseUrl = UexConstants.ApiBaseUrl;
}

internal partial class UexOrganizationsApi
{
    public UexOrganizationsApi(IHttpClientFactory httpClientFactory, IOptions<UexApiOptions> options) : base(httpClientFactory, options)
        => BaseUrl = UexConstants.ApiBaseUrl;
}

internal partial class UexRefineriesApi
{
    public UexRefineriesApi(IHttpClientFactory httpClientFactory, IOptions<UexApiOptions> options) : base(httpClientFactory, options)
        => BaseUrl = UexConstants.ApiBaseUrl;
}

internal partial class UexStaticApi
{
    public UexStaticApi(IHttpClientFactory httpClientFactory, IOptions<UexApiOptions> options) : base(httpClientFactory, options)
        => BaseUrl = UexConstants.ApiBaseUrl;
}

internal partial class UexUserApi
{
    public UexUserApi(IHttpClientFactory httpClientFactory, IOptions<UexApiOptions> options) : base(httpClientFactory, options)
        => BaseUrl = UexConstants.ApiBaseUrl;
}

internal partial class UexVehiclesApi
{
    public UexVehiclesApi(IHttpClientFactory httpClientFactory, IOptions<UexApiOptions> options) : base(httpClientFactory, options)
        => BaseUrl = UexConstants.ApiBaseUrl;
}
