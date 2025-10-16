namespace Arkanis.Overlay.External.CitizenId;

using Common.Extensions;
using Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;

public static class DependencyInjection
{
    public static IServiceCollection AddCitizenIdLinkHelper(this IServiceCollection services)
        => services.AddSingleton<CitizenIdLinkHelper>();

    public static IServiceCollection AddCitizenIdAuthenticatorServices(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddConfiguration<CitizenIdOptions>(configuration)
            .AddSingleton<CitizenIdAuthenticator>()
            .Alias<ExternalAuthenticator, CitizenIdAuthenticator>();
}
