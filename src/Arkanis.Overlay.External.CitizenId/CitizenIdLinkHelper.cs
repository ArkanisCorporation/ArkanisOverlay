namespace Arkanis.Overlay.External.CitizenId;

using Microsoft.Extensions.Options;
using Options;
using Overlay.Common.Options;

public class CitizenIdLinkHelper(IOptionsMonitor<CitizenIdOptions> citizenIdOptions, IOptionsMonitor<ArkanisRestBackendOptions> backendOptions)
{
    public string GetOidcAuthority()
        => citizenIdOptions.CurrentValue.Authority.ToString();

    public string GetLinkAccountUrl()
        => new Uri(backendOptions.CurrentValue.BaseAddress, "/api/v1/overlay/connect/link-account").ToString();
}
