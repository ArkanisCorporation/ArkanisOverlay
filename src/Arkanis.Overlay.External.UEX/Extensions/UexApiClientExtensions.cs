namespace Arkanis.Overlay.External.UEX.Extensions;

using Abstractions;

public static class UexApiClientExtensions
{
    public static TClient WithOverrideOptions<TClient>(this TClient @this, Action<UexApiOptions> configureOptions) where TClient : class, IUexApiClient
    {
        if (@this is not UexApiClientBase sourceClient)
        {
            return @this;
        }

        var newClient = sourceClient.CloneAs<TClient>();
        if (newClient is not UexApiClientBase newApiClient)
        {
            return newClient;
        }

        newApiClient.OverrideOptions = newApiClient.CurrentOptions with { };
        configureOptions(newApiClient.OverrideOptions);

        return newClient;
    }
}
