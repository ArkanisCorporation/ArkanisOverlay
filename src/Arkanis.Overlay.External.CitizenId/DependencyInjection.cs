namespace Arkanis.Overlay.External.CitizenId;

using Common.Extensions;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCitizenIdAuthenticatorServices(this IServiceCollection services)
        => services
            .AddSingleton<CitizenIdAuthenticator>()
            .Alias<ExternalAuthenticator, CitizenIdAuthenticator>();
}
