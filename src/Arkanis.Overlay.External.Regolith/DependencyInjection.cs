namespace Arkanis.Overlay.External.Regolith;

using Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Overlay.Common.Extensions;
using Overlay.Common.Services;
using Services;

using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

public static class DependencyInjection
{
    /// <summary>
    /// Adds the Regolith API client services.
    /// </summary>
    public static IServiceCollection AddRegolithApiClient(this IServiceCollection services)
    {
        // Register HttpClient factory for RegolithApiClient
        services.AddHttpClient<RegolithApiClient>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Register the client as singleton so API key persists across requests
        services.AddSingleton<IRegolithApiClient>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(nameof(RegolithApiClient));
            return ActivatorUtilities.CreateInstance<RegolithApiClient>(sp, httpClient);
        });

        return services;
    }

    /// <summary>
    /// Adds the Regolith authenticator services.
    /// </summary>
    public static IServiceCollection AddRegolithAuthenticator(this IServiceCollection services)
        => services
            .AddSingleton<RegolithAuthenticator>()
            .Alias<ExternalAuthenticator, RegolithAuthenticator>();
}
